using IOBox.Persistence;

namespace IOBox.Queues;

interface IMessageQueue
{
    void EnqueueBatch(IEnumerable<Message> batch);

    List<Message> DequeueBatch(int batchSize);
}
