namespace IOBox.Persistence;

/// <summary>
/// Represents a message retrieved from the db store for processing.
/// </summary>
public record Message
{
    /// <summary>
    /// The internal identifier of the message in the db store. Used to track and 
    /// update the message status (e.g., processed, failed).
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// A unique identifier for the message. Used to ensure idempotency and 
    /// prevent duplicate processing.
    /// </summary>
    public string MessageId { get; init; } = null!;

    /// <summary>
    /// The serialized message. This is the actual payload that will be processed 
    /// by the message processor.
    /// </summary>
    public string Content { get; init; } = null!;

    /// <summary>
    /// Optional metadata about the message context (e.g., queue, topic, producer).
    /// </summary>
    public string? ContextInfo { get; init; }
}
