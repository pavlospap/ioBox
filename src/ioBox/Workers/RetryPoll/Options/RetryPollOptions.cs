using IOBox.TaskExecution.Options;

namespace IOBox.Workers.RetryPoll.Options;

/// <summary>
/// Represents configuration options related to the polling of failed messages, 
/// supporting retry logic.
/// </summary>
public class RetryPollOptions : ITaskExecutionOptions
{
    internal const string Section = "RetryPoll";

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
    /// Default value is <c>5,000 ms (5 sec)</c>.
    /// </summary>
    public int Delay { get; set; } = 5_000;

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

    /// <summary>
    /// The maximum number of retries allowed for a message before it is 
    /// considered permanently failed.
    /// Must be greater than or equal to <c>0</c>.
    /// If set to <c>0</c>, no retries will occur.
    /// Default value is <c>3</c>.
    /// </summary>
    public int Limit { get; set; } = 3;
}
