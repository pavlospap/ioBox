using IOBox.Workers.Expire.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Expire;

static class ExpireExtensions
{
    public static IServiceCollection AddExpireWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IExpireWorker,
            ExpireWorker,
            ExpireOptions,
            ExpireOptionsValidator>(
            section, ioName, ExpireOptions.Section);
    }
}
