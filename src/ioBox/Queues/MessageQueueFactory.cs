using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Queues;

class MessageQueueFactory(IServiceProvider serviceProvider) : IMessageQueueFactory
{
    public IMessageQueue GetOrCreate(string key) =>
        serviceProvider.GetKeyedService<IMessageQueue>(key)!;
}
