using System.Data;

using Dapper;

using IOBox.Persistence;
using IOBox.Persistence.Options;
using IOBox.Workers.Archive.Options;
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
    IOptionsMonitor<ArchiveOptions> archiveOptionsMonitor,
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

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var sql = $@"
            INSERT INTO {table} (
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
                "Message with MessageId: {messageId} already exists in '{ioName}'",
                messageId,
                ioName);
        }
    }

    public async Task<IEnumerable<Message>> GetMessagesToProcessAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var size = pollOptionsMonitor.Get(ioName).BatchSize;

        var sql = $@"
            WITH NewMessages AS ( 
                SELECT TOP (@size) * 
                FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE Status = {New} 
                ORDER BY ReceivedAt 
            ) 
            UPDATE NewMessages 
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
            new { size },
            transaction,
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

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var retryPollOptions = retryPollOptionsMonitor.Get(ioName);

        var size = retryPollOptions.BatchSize;

        var limit = retryPollOptions.Limit;

        var sql = $@"
            WITH FailedMessages AS ( 
                SELECT TOP (@size) * 
                FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    Status = {Failed} AND 
                    Retries < @limit 
                ORDER BY FailedAt 
            ) 
            UPDATE FailedMessages 
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
            new { size, limit },
            transaction,
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

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var sql = $@"
            UPDATE {table} 
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

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var sql = $@"
            UPDATE {table} 
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

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var size = retryPollOptionsMonitor.Get(ioName).BatchSize;

        var timeout = unlockOptionsMonitor.Get(ioName).Timeout;

        var sql = $@"
            WITH LockedMessages AS ( 
                SELECT TOP (@size) * 
                FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    Status = {Locked} AND 
                    LockedAt <= DATEADD(MILLISECOND, -@timeout, SYSUTCDATETIME()) 
                ORDER BY LockedAt 
            ) 
            UPDATE LockedMessages 
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
            new { size, timeout },
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    public async Task MarkMessagesAsExpiredAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var expireOptions = expireOptionsMonitor.Get(ioName);

        var size = expireOptions.BatchSize;

        var newTtl = expireOptions.NewMessageTtl;

        var failedTtl = expireOptions.FailedMessageTtl;

        var sql = $@"
            WITH MessagesToExpire AS ( 
                SELECT TOP (@size) * 
                FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    (Status = {New} AND 
                     ReceivedAt <= DATEADD(MILLISECOND, -@newTtl, SYSUTCDATETIME())) OR
                    (Status = {Failed} AND 
                     FailedAt <= DATEADD(MILLISECOND, -@failedTtl, SYSUTCDATETIME()))
                ORDER BY 
                    CASE 
                        WHEN Status = {New} THEN ReceivedAt
                        WHEN Status = {Failed} THEN FailedAt
                    END
            ) 
            UPDATE MessagesToExpire 
            SET 
                Status = {Expired}, 
                ExpiredAt = SYSUTCDATETIME();";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            new { size, newTtl, failedTtl },
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    public async Task ArchiveMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var dbOptions = dbOptionsMonitor.Get(ioName);

        var table = dbOptions.FullTableName;

        var archiveTable = dbOptions.ArchiveFullTableName;

        var archiveOptions = archiveOptionsMonitor.Get(ioName);

        var size = archiveOptions.BatchSize;

        var processedTtl = archiveOptions.ProcessedMessageTtl;

        var expiredTtl = archiveOptions.ExpiredMessageTtl;

        var sql = $@"
            DECLARE @MessagesToArchive TABLE (Id int);

            INSERT INTO @MessagesToArchive (Id)
            SELECT TOP (@size) Id 
            FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
            WHERE 
                (Status = {Processed} AND 
                    ProcessedAt <= DATEADD(MILLISECOND, -@processedTtl, SYSUTCDATETIME())) OR
                (Status = {Expired} AND 
                    ExpiredAt <= DATEADD(MILLISECOND, -@expiredTtl, SYSUTCDATETIME()))
            ORDER BY 
                CASE 
                    WHEN Status = {Processed} THEN ProcessedAt
                    WHEN Status = {Expired} THEN ExpiredAt
                END;
            
            INSERT INTO {archiveTable} (
                MessageId,
                Message,
                ContextInfo,
                Status,
                Retries,
                Error,
                ReceivedAt,
                LockedAt,
                ProcessedAt,
                FailedAt,
                ExpiredAt)
            SELECT
                MessageId,
                Message,
                ContextInfo,
                Status,
                Retries,
                Error,
                ReceivedAt,
                LockedAt,
                ProcessedAt,
                FailedAt,
                ExpiredAt
            FROM {table}
            WHERE Id IN (SELECT Id FROM @MessagesToArchive);

            DELETE FROM {table}
            WHERE Id IN (SELECT Id FROM @MessagesToArchive);";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            new { size, processedTtl, expiredTtl },
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }

    public async Task DeleteMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(ioName, nameof(ioName));

        var table = dbOptionsMonitor.Get(ioName).FullTableName;

        var deleteOptions = deleteOptionsMonitor.Get(ioName);

        var size = deleteOptions.BatchSize;

        var processedTtl = deleteOptions.ProcessedMessageTtl;

        var expiredTtl = deleteOptions.ExpiredMessageTtl;

        var sql = $@"
            WITH MessagesToDelete AS ( 
                SELECT TOP (@size) * 
                FROM {table} WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    (Status = {Processed} AND 
                     ProcessedAt <= DATEADD(MILLISECOND, -@processedTtl, SYSUTCDATETIME())) OR
                    (Status = {Expired} AND 
                     ExpiredAt <= DATEADD(MILLISECOND, -@expiredTtl, SYSUTCDATETIME()))
                ORDER BY 
                    CASE 
                        WHEN Status = {Processed} THEN ProcessedAt
                        WHEN Status = {Expired} THEN ExpiredAt
                    END
            ) 
            DELETE FROM MessagesToDelete;";

        using var connection = dbContext.CreateConnection(ioName);

        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.ReadCommitted);

        var command = new CommandDefinition(
            sql,
            new { size, processedTtl, expiredTtl },
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        transaction.Commit();
    }
}
