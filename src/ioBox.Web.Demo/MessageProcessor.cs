using Dapper;

using EasyNetQ;

using IOBox.Persistence;

using Microsoft.Data.SqlClient;

namespace IOBox.Web.Demo;

class MessageProcessor(
    IBus bus,
    IDbStore dbStore,
    IConfiguration configuration) : IMessageProcessor
{
    public async Task ProcessMessagesAsync(
        string ioName,
        List<Message> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            try
            {
                using var connection = new SqlConnection(
                    configuration.GetConnectionString("DbConnection"));

                await connection.OpenAsync(cancellationToken);

                using var transaction = connection.BeginTransaction();

                switch (ioName)
                {
                    case "Inbox01":
                        {
                            var msg = JsonSerializer.Deserialize<NewMemberWelcomeMessage>(
                                message.Content);

                            var sql = @"
                                UPDATE App.Members 
                                SET HasBeenWelcomed = 1
                                WHERE Id = @MemberId";

                            await connection.ExecuteAsync(
                                sql,
                                new { msg!.MemberId },
                                transaction);

                            await dbStore.MarkMessageAsProcessedAsync(
                                message.Id,
                                ioName,
                                connection,
                                transaction,
                                cancellationToken);

                            break;
                        }
                    case "Inbox02":
                        {
                            var msg = JsonSerializer.Deserialize<NewMemberBonusMessage>(
                                message.Content);

                            var sql = @"
                                UPDATE App.Members 
                                SET HasBeenGivenBonus = 1
                                WHERE Id = @MemberId";

                            await connection.ExecuteAsync(
                                sql,
                                new { msg!.MemberId },
                                transaction);

                            await dbStore.MarkMessageAsProcessedAsync(
                                message.Id,
                                ioName,
                                connection,
                                transaction,
                                cancellationToken);

                            break;
                        }
                    case "Outbox01":
                        {
                            var msg = JsonSerializer.Deserialize<NewMemberWelcomeMessage>(
                                message.Content);

                            await bus.SendReceive.SendAsync(
                                nameof(NewMemberWelcomeMessage), msg, cancellationToken);

                            await dbStore.MarkMessageAsProcessedAsync(
                                message.Id,
                                ioName,
                                connection,
                                transaction,
                                cancellationToken);

                            break;
                        }
                    case "Outbox02":
                        {
                            var msg = JsonSerializer.Deserialize<NewMemberBonusMessage>(
                                message.Content);

                            await bus.SendReceive.SendAsync(
                                nameof(NewMemberBonusMessage), msg, cancellationToken);

                            await dbStore.MarkMessageAsProcessedAsync(
                                message.Id,
                                ioName,
                                connection,
                                transaction,
                                cancellationToken);

                            break;
                        }

                    default:
                        throw new NotSupportedException(
                            $"Processing for {ioName} is not supported.");
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                await dbStore.MarkMessageAsFailedAsync(
                    message.Id,
                    ioName,
                    ex.Message,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
