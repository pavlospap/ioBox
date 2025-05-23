namespace IOBox.Queues;

interface IMessageQueueFactory
{
    IMessageQueue GetOrCreate(string key);
}
