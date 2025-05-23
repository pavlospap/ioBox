using IOBox.Persistence;
using IOBox.TaskExecution;
using IOBox.Workers.Expire.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Expire;

class ExpireWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<ExpireOptions> expireOptionsMonitor,
    ITaskExecutionWrapper taskExecutionWrapper) : IExpireWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task expireAsync(CancellationToken cancellationToken) =>
            dbStore.MarkMessagesAsExpiredAsync(ioName, cancellationToken);

        return taskExecutionWrapper.WrapTaskAsync(
            expireAsync,
            expireOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
