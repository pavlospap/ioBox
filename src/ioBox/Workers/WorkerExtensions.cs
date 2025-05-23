using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace IOBox.Workers;

static class WorkerExtensions
{
    public static IServiceCollection AddWorker
        <TWorker, TWorkerImpl, TOptions, TOptionsValidator>(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName,
        string optionsSectionName)
        where TWorker : class, IWorker
        where TWorkerImpl : TWorker
        where TOptions : class
        where TOptionsValidator : class, IValidateOptions<TOptions>
    {
        var optionsSection = section.GetSection(optionsSectionName);

        services
            .AddKeyedSingleton<TWorker>(ioName, (sp, key) =>
                ActivatorUtilities.CreateInstance<TWorkerImpl>(sp, key))
            .AddSingleton<IWorker>(sp => sp.GetKeyedService<TWorker>(ioName)!)
            .AddOptions<TOptions>(ioName)
            .Bind(optionsSection)
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<TOptions>, TOptionsValidator>();

        return services;
    }
}
