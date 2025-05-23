using IOBox.Persistence;
using IOBox.Queues;
using IOBox.TaskExecution;
using IOBox.Workers.RetryPoll.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.RetryPoll;

class RetryPollWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<RetryPollOptions> retryPollOptionsMonitor,
    IMessageQueueFactory messageQueueFactory,
    ITaskExecutionWrapper taskExecutionWrapper) : IRetryPollWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task retryPollAsync(CancellationToken cancellationToken)
        {
            var messages = await dbStore.GetMessagesToRetryAsync(
                ioName, cancellationToken);

            messageQueueFactory.GetOrCreate(ioName).EnqueueBatch(messages);
        }

        return taskExecutionWrapper.WrapTaskAsync(
            retryPollAsync,
            retryPollOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
