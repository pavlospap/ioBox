using System.Data;

using Dapper;

using IOBox.Persistence;
using IOBox.Persistence.Options;
using IOBox.Workers.Delete.Options;
using IOBox.Workers.Expire.Options;
using IOBox.Workers.Poll.Options;
using IOBox.Workers.RetryPoll.Options;
using IOBox.Workers.Unlock.Options;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IOBox.SqlServer;

class SqlServerDbStore(
    IDbContext dbContext,
    ILogger<SqlServerDbStore> logger,
    IOptionsMonitor<DbOptions> dbOptionsMonitor,
    IOptionsMonitor<PollOptions> pollOptionsMonitor,
    IOptionsMonitor<RetryPollOptions> retryPollOptionsMonitor,
    IOptionsMonitor<UnlockOptions> unlockOptionsMonitor,
    IOptionsMonitor<ExpireOptions> expireOptionsMonitor,
    IOptionsMonitor<DeleteOptions> deleteOptionsMonitor) : IDbStore
{
    const byte New = 1;
    const byte Locked = 2;
    const byte Processed = 3;
    const byte Failed = 4;
    const byte Expired = 5;

    const short UniqueIndexViolation = 2601;

    public async Task AddNewMessageAsync(
        string messageId,
        string message,
        string ioName,
        string? contextInfo = null,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

        ArgumentException.ThrowIfNullOrEmpty(messageId, nameof(messageId));

        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        if (transaction is not null && connection is null)
        {
            throw new ArgumentException(
                "A transaction was provided without a corresponding connection.",
                nameof(transaction));
        }

        var sql = $@"
            INSERT INTO {TableName(ioName)} (
                MessageId, 
                Message, 
                ContextInfo, 
                Status, 
                ReceivedAt, 
                Retries) 
            VALUES (
                @messageId, 
                @message, 
                @contextInfo, 
                {New}, 
                SYSUTCDATETIME(), 
                0);";

        var command = new CommandDefinition(
            sql,
            new { messageId, message, contextInfo },
            transaction,
            cancellationToken: cancellationToken);

        try
        {
            if (connection is null)
            {
                using var conn = dbContext.CreateConnection(ioName);

                await conn.ExecuteAsync(command);

                return;
            }

            await connection.ExecuteAsync(command);
        }
        catch (SqlException ex) when (ex.Number == UniqueIndexViolation)
        {
            logger.LogError(
                "Message with MessageId: {messageId} already exists in '{ioName}'.",
                messageId,
                ioName);
        }
    }

    public async Task<IEnumerable<Message>> GetMessagesToProcessAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var size = pollOptionsMonitor.Get(ioName).BatchSize;

        var sql = $@"
            WITH CTE AS ( 
                SELECT TOP ({size}) * 
                FROM {TableName(ioName)} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE Status = {New} 
                ORDER BY ReceivedAt 
            ) 
            UPDATE CTE 
            SET 
                Status = {Locked}, 
                LockedAt = SYSUTCDATETIME() 
            OUTPUT 
                INSERTED.Id, 
                INSERTED.MessageId, 
                INSERTED.Message AS Content, 
                INSERTED.ContextInfo;";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        var messages = await connection.QueryAsync<Message>(command);

        transaction.Commit();

        return messages;
    }

    public async Task<IEnumerable<Message>> GetMessagesToRetryAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var retryPollOptions = retryPollOptionsMonitor.Get(ioName);

        var size = retryPollOptions.BatchSize;

        var limit = retryPollOptions.Limit;

        var sql = $@"
            WITH CTE AS ( 
                SELECT TOP ({size}) * 
                FROM {TableName(ioName)} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    Status = {Failed} AND 
                    Retries < {limit} 
                ORDER BY FailedAt 
            ) 
            UPDATE CTE 
            SET 
                Status = {Locked}, 
                LockedAt = SYSUTCDATETIME(), 
                Retries = Retries + 1 
            OUTPUT 
                INSERTED.Id, 
                INSERTED.MessageId,
                INSERTED.Message AS Content,
                INSERTED.ContextInfo;";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        var messages = await connection.QueryAsync<Message>(command);

        transaction.Commit();

        return messages;
    }

    public async Task MarkMessageAsProcessedAsync(
        int id,
        string ioName,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        if (transaction is not null && connection is null)
        {
            throw new ArgumentException(
                "A transaction was provided without a corresponding connection.",
                nameof(transaction));
        }

        var sql = $@"
            UPDATE {TableName(ioName)} 
            SET 
                Status = {Processed}, 
                ProcessedAt = SYSUTCDATETIME(),
                LockedAt = NULL
            WHERE 
                Id = @id;";

        var command = new CommandDefinition(
            sql,
            new { id },
            transaction,
            cancellationToken: cancellationToken);

        if (connection is null)
        {
            using var conn = dbContext.CreateConnection(ioName);

            await conn.ExecuteAsync(command);

            return;
        }

        await connection.ExecuteAsync(command);
    }

    public async Task MarkMessageAsFailedAsync(
        int id,
        string ioName,
        string? error = null,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        if (transaction is not null && connection is null)
        {
            throw new ArgumentException(
                "A transaction was provided without a corresponding connection.",
                nameof(transaction));
        }

        var sql = $@"
            UPDATE {TableName(ioName)} 
            SET 
                Status = {Failed},
                Error = @error,
                FailedAt = SYSUTCDATETIME(),
                LockedAt = NULL
            WHERE 
                Id = @id;";

        var command = new CommandDefinition(
            sql,
            new { id, error },
            transaction,
            cancellationToken: cancellationToken);

        if (connection is null)
        {
            using var conn = dbContext.CreateConnection(ioName);

            await conn.ExecuteAsync(command);

            return;
        }

        await connection.ExecuteAsync(command);
    }

    public async Task UnlockMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var size = retryPollOptionsMonitor.Get(ioName).BatchSize;

        var timeout = unlockOptionsMonitor.Get(ioName).Timeout;

        var sql = $@"
            WITH CTE AS ( 
                SELECT TOP ({size}) * 
                FROM {TableName(ioName)} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    Status = {Locked} AND 
                    LockedAt <= DATEADD(MILLISECOND, -{timeout}, SYSUTCDATETIME()) 
                ORDER BY LockedAt 
            ) 
            UPDATE CTE 
            SET 
                Status = {Failed}, 
                FailedAt = SYSUTCDATETIME(), 
                LockedAt = NULL;";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    public async Task MarkMessagesAsExpiredAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var expireOptions = expireOptionsMonitor.Get(ioName);

        var size = expireOptions.BatchSize;

        var failedTtl = expireOptions.FailedMessageTtl;

        var newTtl = expireOptions.NewMessageTtl;

        var limit = retryPollOptionsMonitor.Get(ioName).Limit;

        var sql = $@"
            WITH CTE AS ( 
                SELECT TOP ({size}) * 
                FROM {TableName(ioName)} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    (Status = {New} AND 
                     ReceivedAt <= DATEADD(MILLISECOND, -{newTtl}, SYSUTCDATETIME())) OR
                    (Status = {Failed} AND 
                     FailedAt <= DATEADD(MILLISECOND, -{failedTtl}, SYSUTCDATETIME()))
                ORDER BY 
                    CASE 
                        WHEN Status = {New} THEN ReceivedAt
                        WHEN Status = {Failed} THEN FailedAt
                    END
            ) 
            UPDATE CTE 
            SET 
                Status = {Expired}, 
                ExpiredAt = SYSUTCDATETIME();";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    public Task ArchiveMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        return Task.CompletedTask;
    }

    public async Task DeleteMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var deleteOptions = deleteOptionsMonitor.Get(ioName);

        var size = deleteOptions.BatchSize;

        var expiredTtl = deleteOptions.ExpiredMessageTtl;

        var processedTtl = deleteOptions.ProcessedMessageTtl;

        var sql = $@"
            WITH CTE AS ( 
                SELECT TOP ({size}) * 
                FROM {TableName(ioName)} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    (Status = {Processed} AND 
                     ProcessedAt <= DATEADD(MILLISECOND, -{processedTtl}, SYSUTCDATETIME())) OR
                    (Status = {Expired} AND 
                     ExpiredAt <= DATEADD(MILLISECOND, -{expiredTtl}, SYSUTCDATETIME()))
                ORDER BY 
                    CASE 
                        WHEN Status = {Processed} THEN ProcessedAt
                        WHEN Status = {Expired} THEN ExpiredAt
                    END
            ) 
            DELETE FROM CTE;";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    string TableName(string ioName)
    {
        var options = dbOptionsMonitor.Get(ioName);

        return options.SchemaName + "." + options.TableName;
    }
}
