using IOBox.Persistence;
using IOBox.TaskExecution;
using IOBox.Workers.Unlock.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Unlock;

class UnlockWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<UnlockOptions> unlockOptionsMonitor,
    ITaskExecutionWrapper taskExecutionWrapper) : IUnlockWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task unlockAsync(CancellationToken cancellationToken) =>
            dbStore.UnlockMessagesAsync(ioName, cancellationToken);

        return taskExecutionWrapper.WrapTaskAsync(
            unlockAsync,
            unlockOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
