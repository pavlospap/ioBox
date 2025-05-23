using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace IOBox.Persistence.Options;

static class DbOptionsExtensions
{
    public static IServiceCollection AddDbOptions(
        this IServiceCollection services,
        IConfigurationSection section,
        string ioName)
    {
        var dbSection = section.GetSection(DbOptions.Section);

        services
            .AddOptions<DbOptions>(ioName)
            .Bind(dbSection)
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<DbOptions>, DbOptionsValidator>();

        return services;
    }
}
