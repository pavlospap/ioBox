using IOBox.Persistence;

namespace IOBox;

/// <summary>
/// Defines the contract for processing messages retrieved by inbox/outbox 
/// operations. This interface must be implemented by consumers to provide custom 
/// message handling logic, but it is intended to be invoked only by the library 
/// infrastructure — not by library consumers.
/// </summary>
public interface IMessageProcessor
{
    /// <summary>
    /// Processes a batch of messages retrieved from an inbox or outbox db store.
    /// This method is called by the library infrastructure as part of the message 
    /// processing pipeline. Implementations should provide the logic for handling 
    /// each message. This method should not be called directly by library consumers.
    /// </summary>
    /// <param name="name">The name of the inbox/outbox.</param>
    /// <param name="messages">The list of messages to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessMessagesAsync(
        string name,
        List<Message> messages,
        CancellationToken cancellationToken = default);
}
