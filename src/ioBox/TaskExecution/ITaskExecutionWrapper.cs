using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Options;

namespace IOBox.TaskExecution;

interface ITaskExecutionWrapper
{
    Task WrapTaskAsync<TOptions>(
        Func<CancellationToken, Task> task,
        IOptionsMonitor<TOptions> optionsMonitor,
        string ioName,
        CancellationToken stoppingToken) where TOptions : ITaskExecutionOptions;
}
