using IOBox.Persistence;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox.SqlServer;

/// <summary>
/// Provides extension methods for registering ioBox SQL Server-related services.
/// </summary>
public static class Services
{
    /// <summary>
    /// Registers the necessary services for using SQL Server as the underlying
    /// persistence layer for ioBox.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddIOBoxSqlServer(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        return services
            .AddScoped<IDbMigrator, SqlServerDbMigrator>()
            .AddSingleton<IDbContext, SqlServerDbContext>()
            .AddSingleton<IDbStore, SqlServerDbStore>();
    }
}
