using IOBox.Workers.RetryPoll.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.RetryPoll;

static class RetryPollExtensions
{
    public static IServiceCollection AddRetryPollWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IRetryPollWorker,
            RetryPollWorker,
            RetryPollOptions,
            RetryPollOptionsValidator>(
            section, ioName, RetryPollOptions.Section);
    }
}
