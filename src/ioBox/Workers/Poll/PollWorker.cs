using IOBox.Persistence;
using IOBox.Queues;
using IOBox.TaskExecution;
using IOBox.Workers.Poll.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Poll;

class PollWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<PollOptions> pollOptionsMonitor,
    IMessageQueueFactory messageQueueFactory,
    ITaskExecutionWrapper taskExecutionWrapper) : IPollWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task pollAsync(CancellationToken cancellationToken)
        {
            var messages = await dbStore.GetMessagesToProcessAsync(
                ioName, cancellationToken);

            messageQueueFactory.GetOrCreate(ioName).EnqueueBatch(messages);
        }

        return taskExecutionWrapper.WrapTaskAsync(
            pollAsync,
            pollOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
