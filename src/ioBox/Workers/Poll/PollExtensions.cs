using IOBox.Workers.Poll.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Poll;

static class PollExtensions
{
    public static IServiceCollection AddPollWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IPollWorker,
            PollWorker,
            PollOptions,
            PollOptionsValidator>(
            section, ioName, PollOptions.Section);
    }
}
