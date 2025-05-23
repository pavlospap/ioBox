using IOBox.TaskExecution.Options;

namespace IOBox.Workers.Process.Options;

/// <summary>
/// Represents configuration options related to the processing of new messages.
/// </summary>
public class ProcessOptions : ITaskExecutionOptions
{
    internal const string Section = "Process";

    /// <summary>
    /// Indicates whether this worker is enabled and should perform its task.
    /// The worker is always scheduled to run at the specified <see cref="Delay"/>, 
    /// but it will only execute its logic if this setting is <c>true</c>.
    /// Default value is <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The delay, in milliseconds, between each execution cycle.
    /// Must be greater than <c>0</c>.
    /// Default value is <c>1,000 ms (1 sec)</c>.
    /// </summary>
    public int Delay { get; set; } = 1_000;

    /// <summary>
    /// The timeout, in milliseconds, for a single execution cycle.
    /// Must be <c>null</c> or greater than <c>0</c>.
    /// If set to <c>null</c>, no timeout will be applied.
    /// Default value is <c>10,000 ms (10 sec)</c>.
    /// </summary>
    public int? Timeout { get; set; } = 10_000;

    /// <summary>
    /// The batch size, i.e., the number of messages to process in a single 
    /// execution cycle.
    /// Must be greater than <c>0</c>.
    /// Default value is <c>100</c>.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
