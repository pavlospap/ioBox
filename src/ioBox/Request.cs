using IOBox.Persistence;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox;

/// <summary>
/// Provides extension methods to integrate database-related functionality for 
/// ioBox during application startup.
/// </summary>
public static class Request
{
    /// <summary>
    /// Applies ioBox database integration to the web application by executing
    /// database migrations for all configured inboxes and outboxes.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static WebApplication UseIOBox(
        this WebApplication app, IConfiguration configuration)
    {
        var names = configuration.GetAllInboxOutboxSections()
            .Select(s => s.GetValue<string>("Name")!);

        using var scope = app.Services.CreateScope();

        var dbMigrator = scope.ServiceProvider.GetRequiredService<IDbMigrator>();

        foreach (var name in names)
        {
            dbMigrator.MigrateDb(name);
        }

        return app;
    }
}
