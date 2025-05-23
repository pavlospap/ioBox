using IOBox.Persistence;
using IOBox.TaskExecution;
using IOBox.Workers.Archive.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Archive;

class ArchiveWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<ArchiveOptions> archiveOptionsMonitor,
    ITaskExecutionWrapper taskExecutionWrapper) : IArchiveWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task archiveAsync(CancellationToken cancellationToken) =>
            dbStore.ArchiveMessagesAsync(ioName, cancellationToken);

        return taskExecutionWrapper.WrapTaskAsync(
            archiveAsync,
            archiveOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
