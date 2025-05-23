using IOBox.Workers.Delete.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.Workers.Delete;

static class DeleteExtensions
{
    public static IServiceCollection AddDeleteWorker(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        return services.AddWorker<
            IDeleteWorker,
            DeleteWorker,
            DeleteOptions,
            DeleteOptionsValidator>(
            section, ioName, DeleteOptions.Section);
    }
}
