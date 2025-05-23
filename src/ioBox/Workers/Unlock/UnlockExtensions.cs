using IOBox.Workers.Unlock.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Unlock;

static class UnlockExtensions
{
    public static IServiceCollection AddUnlockWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IUnlockWorker,
            UnlockWorker,
            UnlockOptions,
            UnlockOptionsValidator>(
            section, ioName, UnlockOptions.Section);
    }
}
