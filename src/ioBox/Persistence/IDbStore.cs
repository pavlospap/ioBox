using System.Data;

namespace IOBox.Persistence;

/// <summary>
/// Represents a contract for storing and managing messages in a persistent database.
/// </summary>
public interface IDbStore
{
    /// <summary>
    /// Adds a new message to the db store. This method is intended to be invoked by 
    /// library consumers.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message used for deduplication.</param>
    /// <param name="message">The serialized message content.</param>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="contextInfo">Optional context information related to the message.</param>
    /// <param name="connection">
    /// An optional database connection to use for the operation. The caller is 
    /// responsible for managing its lifecycle.
    /// </param>
    /// <param name="transaction">
    /// An optional database transaction to use for the operation. If provided, it 
    /// must be associated with the specified <paramref name="connection"/>. The 
    /// caller is responsible for managing its lifecycle.
    /// </param> 
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddNewMessageAsync(
        string messageId,
        string message,
        string ioName,
        string? contextInfo = null,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves messages that are ready to be processed. This method is intended 
    /// to be invoked only by the library infrastructure — not by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of messages ready for processing.</returns>
    Task<IEnumerable<Message>> GetMessagesToProcessAsync(
        string ioName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves messages that are eligible to be retried after a failure. This 
    /// method is intended to be invoked only by the library infrastructure — not 
    /// by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of messages eligible for retry.</returns>
    Task<IEnumerable<Message>> GetMessagesToRetryAsync(
        string ioName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified message as successfully processed. This method is 
    /// intended to be invoked by library consumers.
    /// </summary>
    /// <param name="id">The internal identifier of the message in the db store.</param>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="connection">
    /// An optional database connection to use for the operation. The caller is 
    /// responsible for managing its lifecycle.
    /// </param>
    /// <param name="transaction">
    /// An optional database transaction to use for the operation. If provided, it 
    /// must be associated with the specified <paramref name="connection"/>. The 
    /// caller is responsible for managing its lifecycle.
    /// </param> 
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkMessageAsProcessedAsync(
        int id,
        string ioName,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified message as failed to process. This method is intended 
    /// to be invoked by library consumers.
    /// </summary>
    /// <param name="id">The internal identifier of the message in the db store.</param>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="connection">
    /// An optional database connection to use for the operation. The caller is 
    /// responsible for managing its lifecycle.
    /// </param>
    /// <param name="transaction">
    /// An optional database transaction to use for the operation. If provided, it 
    /// must be associated with the specified <paramref name="connection"/>. The 
    /// caller is responsible for managing its lifecycle.
    /// </param> 
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkMessageAsFailedAsync(
        int id,
        string ioName,
        string? error = null,
        IDbConnection? connection = null,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks messages that were previously locked for processing but were not 
    /// completed.This method is intended to be invoked only by the library 
    /// infrastructure — not by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UnlockMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks new and failed messages as expired based on the TTLs specified on 
    /// configuration. This method is intended to be invoked only by the library 
    /// infrastructure — not by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkMessagesAsExpiredAsync(
        string ioName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives processed and expired messages based on the TTLs specified on 
    /// configuration. This method is intended to be invoked only by the library 
    /// infrastructure — not by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ArchiveMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes processed and expired messages based on the TTLs specified 
    /// on configuration. This method is intended to be invoked only by the library 
    /// infrastructure — not by library consumers.
    /// </summary>
    /// <param name="ioName">The inbox/outbox name to get the related configuration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteMessagesAsync(
        string ioName,
        CancellationToken cancellationToken = default);
}
