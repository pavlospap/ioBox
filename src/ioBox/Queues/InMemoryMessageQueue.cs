using System.Collections.Concurrent;

using IOBox.Persistence;

namespace IOBox.Queues;

class InMemoryMessageQueue : IMessageQueue
{
    readonly ConcurrentQueue<Message> _queue = new();

    public void EnqueueBatch(IEnumerable<Message> batch)
    {
        foreach (var message in batch)
        {
            _queue.Enqueue(message);
        }
    }

    public List<Message> DequeueBatch(int batchSize)
    {
        var batch = new List<Message>(batchSize);

        while (batch.Count < batchSize && _queue.TryDequeue(out var message))
        {
            batch.Add(message);
        }

        return batch;
    }
}
