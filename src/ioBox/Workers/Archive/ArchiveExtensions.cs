using IOBox.Workers.Archive.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Archive;

static class ArchiveExtensions
{
    public static IServiceCollection AddArchiveWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IArchiveWorker,
            ArchiveWorker,
            ArchiveOptions,
            ArchiveOptionsValidator>(
            section, ioName, ArchiveOptions.Section);
    }
}
