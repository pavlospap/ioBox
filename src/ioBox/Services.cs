using IOBox.Persistence.Options;
using IOBox.Queues;
using IOBox.TaskExecution;
using IOBox.Workers.Archive;
using IOBox.Workers.Delete;
using IOBox.Workers.Expire;
using IOBox.Workers.Poll;
using IOBox.Workers.Process;
using IOBox.Workers.RetryPoll;
using IOBox.Workers.Unlock;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOBox;

/// <summary>
/// Provides extension methods for registering ioBox services and configurations 
/// related to inbox/outbox message processing.
/// </summary>
public static class Services
{
    /// <summary>
    /// Registers all services and background workers required for ioBox inbox/outbox
    /// processing, including polling, processing, retrying, expiring, unlocking, archiving
    /// and deleting messages.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if no inboxes or outboxes are defined, or if names are missing or duplicated.
    /// </exception>
    public static IServiceCollection AddIOBox(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        ValidateNames(configuration);

        var inboxOutboxSections = configuration.GetAllInboxOutboxSections();

        foreach (var section in inboxOutboxSections)
        {
            var name = section.GetValue<string>("Name")!;

            var workersSection = section.GetSection(
                ConfigurationExtensions.WorkersSection);

            services
                .AddPollWorker(workersSection, name)
                .AddRetryPollWorker(workersSection, name)
                .AddProcessWorker(workersSection, name)
                .AddUnlockWorker(workersSection, name)
                .AddExpireWorker(workersSection, name)
                .AddArchiveWorker(workersSection, name)
                .AddDeleteWorker(workersSection, name)
                .AddKeyedSingleton<IMessageQueue, InMemoryMessageQueue>(name)
                .AddDbOptions(section, name);
        }

        return services
            .AddHostedService<MessageBackgroundService>()
            .AddSingleton<IMessageQueueFactory, MessageQueueFactory>()
            .AddSingleton<ITaskExecutionWrapper, TaskExecutionWrapper>();
    }

    static void ValidateNames(IConfiguration configuration)
    {
        var names = configuration.GetAllInboxOutboxSections()
            .Select(s => s.GetValue<string>("Name"));

        if (!names.Any())
        {
            throw new ArgumentException(
                "Configuration must define at least one Inbox or Outbox under " +
                $"'{ConfigurationExtensions.InboxesSection}' or " +
                $"'{ConfigurationExtensions.OutboxesSection}'.");
        }

        if (names.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "Each Inbox or Outbox configuration must have a non-empty 'Name' value.");
        }

        var hasDuplicateNames = names
            .GroupBy(n => n)
            .Any(g => g.Count() > 1);

        if (hasDuplicateNames)
        {
            throw new ArgumentException(
                "Duplicate Inbox or Outbox names detected. " +
                "Each configuration must have a unique 'Name' value.");
        }
    }
}
