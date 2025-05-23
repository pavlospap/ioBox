using Microsoft.Extensions.Configuration;

namespace IOBox;

static class ConfigurationExtensions
{
    public const string InboxesSection = "IOBox:Inboxes";

    public const string OutboxesSection = "IOBox:Outboxes";

    public const string WorkersSection = "Workers";

    public static IEnumerable<IConfigurationSection> GetAllInboxOutboxSections(
        this IConfiguration configuration)
    {
        var inboxes = configuration
            .GetSection(InboxesSection)
            .GetChildren();

        var outboxes = configuration
            .GetSection(OutboxesSection)
            .GetChildren();

        return inboxes.Concat(outboxes);
    }
}
