using IOBox.TaskExecution.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IOBox.TaskExecution;

class TaskExecutionWrapper(ILogger<TaskExecutionWrapper> logger) :
    ITaskExecutionWrapper
{
    public async Task WrapTaskAsync<TOptions>(
        Func<CancellationToken, Task> task,
        IOptionsMonitor<TOptions> optionsMonitor,
        string ioName,
        CancellationToken stoppingToken) where TOptions : ITaskExecutionOptions
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = optionsMonitor.Get(ioName);

            using var cts = options.Timeout.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(stoppingToken)
                : null;

            cts?.CancelAfter(options.Timeout!.Value);

            var token = cts?.Token ?? stoppingToken;

            try
            {
                if (options.Enabled)
                {
                    await task(token);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(options.Delay), stoppingToken);
            }
            catch (OperationCanceledException ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(
                    ex,
                    "Task execution was cancelled unexpectedly for {workerName}",
                    task.Method.DeclaringType!.Name);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An unhandled exception occurred while executing {workerName}",
                    task.Method.DeclaringType!.Name);
            }
        }
    }
}
