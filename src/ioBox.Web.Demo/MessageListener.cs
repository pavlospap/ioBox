using EasyNetQ;

using IOBox.Persistence;

namespace IOBox.Web.Demo;

public class MessageListener(
    IBus bus,
    IDbStore dbStore) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await bus.SendReceive.ReceiveAsync<NewMemberWelcomeMessage>(
            nameof(NewMemberWelcomeMessage),
            async message => await dbStore.AddNewMessageAsync(
                messageId: message.MessageId,
                message: JsonSerializer.Serialize(message),
                ioName: "Inbox01",
                contextInfo: $"Queue: {nameof(NewMemberWelcomeMessage)}",
                cancellationToken: stoppingToken),
            cancellationToken: stoppingToken);

        await bus.SendReceive.ReceiveAsync<NewMemberBonusMessage>(
            nameof(NewMemberBonusMessage),
            async message => await dbStore.AddNewMessageAsync(
                messageId: message.MessageId,
                message: JsonSerializer.Serialize(message),
                ioName: "Inbox02",
                contextInfo: $"Queue: {nameof(NewMemberBonusMessage)}",
                cancellationToken: stoppingToken),
            cancellationToken: stoppingToken);
    }
}
