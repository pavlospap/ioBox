using IOBox.Persistence;
using IOBox.TaskExecution;
using IOBox.Workers.Delete.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Workers.Delete;

class DeleteWorker(
    string ioName,
    IDbStore dbStore,
    IOptionsMonitor<DeleteOptions> deleteOptionsMonitor,
    ITaskExecutionWrapper taskExecutionWrapper) : IDeleteWorker
{
    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task deleteAsync(CancellationToken cancellationToken) =>
            dbStore.DeleteMessagesAsync(ioName, cancellationToken);

        return taskExecutionWrapper.WrapTaskAsync(
            deleteAsync,
            deleteOptionsMonitor,
            ioName,
            stoppingToken);
    }
}
