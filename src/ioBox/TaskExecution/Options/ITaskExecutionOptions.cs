namespace IOBox.TaskExecution.Options;

/// <summary>
/// A contract representing common execution options used in inbox/outbox operations.
/// </summary>
public interface ITaskExecutionOptions
{
    /// <summary>
    /// Indicates whether this worker is enabled and should perform its task.
    /// The worker is always scheduled to run at the specified <see cref="Delay"/>, 
    /// but it will only execute its logic if this setting is <c>true</c>.
    /// Default value depends on the type of operation being performed.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// The delay, in milliseconds, between each execution cycle.
    /// Must be greater than <c>0</c>.
    /// Default value depends on the type of operation being performed.
    /// </summary>
    int Delay { get; set; }

    /// <summary>
    /// The timeout, in milliseconds, for a single execution cycle.
    /// Must be <c>null</c> or greater than <c>0</c>.
    /// If set to <c>null</c>, no timeout will be applied.
    /// Default value depends on the type of operation being performed.
    /// </summary>
    int? Timeout { get; set; }

    /// <summary>
    /// The batch size, i.e., the number of messages to process in a single 
    /// execution cycle.
    /// Must be greater than <c>0</c>.
    /// Default value depends on the type of operation being performed.
    /// </summary>
    int BatchSize { get; set; }
}
