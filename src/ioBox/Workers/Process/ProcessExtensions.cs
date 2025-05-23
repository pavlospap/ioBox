using IOBox.Workers.Process.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Process;

static class ProcessExtensions
{
    public static IServiceCollection AddProcessWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IProcessWorker,
            ProcessWorker,
            ProcessOptions,
            ProcessOptionsValidator>(
            section, ioName, ProcessOptions.Section);
    }
}
