using IOBox.Workers.Archive.Options;

using Microsoft.Extensions.Options;

namespace IOBox.Persistence.Options;

class DbOptionsValidator(IOptionsMonitor<ArchiveOptions> archiveOptionsMonitor) :
    IValidateOptions<DbOptions>
{
    public ValidateOptionsResult Validate(string? name, DbOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.ConnectionString)} must have a non-empty value.");
        }

        if (options.CreateDatabaseIfNotExists &&
            string.IsNullOrWhiteSpace(options.DefaultConnectionString))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.DefaultConnectionString)} must have a non-empty value.");
        }

        if (options.CreateDatabaseIfNotExists &&
            string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.DatabaseName)} must have a non-empty value.");
        }

        if (string.IsNullOrWhiteSpace(options.SchemaName))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.SchemaName)} must have a non-empty value.");
        }

        if (string.IsNullOrWhiteSpace(options.TableName))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.TableName)} must have a non-empty value.");
        }

        var archiveOptions = archiveOptionsMonitor.Get(name!);

        if (archiveOptions.Enabled &&
            string.IsNullOrWhiteSpace(options.ArchiveTableName))
        {
            return ValidateOptionsResult.Fail(
                $"{name} - {nameof(DbOptions)}." +
                $"{nameof(DbOptions.ArchiveTableName)} must have a non-empty value.");
        }

        return ValidateOptionsResult.Success;
    }
}
