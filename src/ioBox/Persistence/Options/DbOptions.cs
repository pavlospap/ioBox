namespace IOBox.Persistence.Options;

/// <summary>
/// Represents configuration options for database access and initialization.
/// </summary>
public class DbOptions
{
    internal const string Section = "Database";

    /// <summary>
    /// The connection string to the target application database.
    /// This is required for any database interaction.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Connection string used for administrative operations such as 
    /// creating the database when <see cref="CreateDatabaseIfNotExists"/> is <c>true</c>.
    /// For example, this can point to the <c>master</c> database in SQL Server or 
    /// the <c>postgres</c> database in PostgreSQL.
    /// This property is required if database creation is enabled.
    /// </summary>
    public string? DefaultConnectionString { get; set; }

    /// <summary>
    /// The name of the database to use.
    /// If the specified database does not exist and <see cref="CreateDatabaseIfNotExists"/> 
    /// is set to <c>true</c> it will be created automatically.
    /// This property is required if database creation is enabled.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// The name of the database schema to use. This property is required.
    /// If the specified schema does not exist and <see cref="CreateSchemaIfNotExists"/> 
    /// is set to <c>true</c> it will be created automatically.
    /// To use the default schema (e.g., <c>dbo</c> for SQL Server or <c>public</c> 
    /// for PostgreSQL), explicitly provide its name instead of leaving this value 
    /// null or empty.
    /// </summary>
    public string SchemaName { get; set; } = null!;

    /// <summary>
    /// The name of the table used for storing messages.
    /// This property is required.
    /// </summary>
    public string TableName { get; set; } = null!;

    /// <summary>
    /// If specified, the table will be created and used to store archived messages 
    /// when <see cref="Workers.Archive.Options.ArchiveOptions.Enabled"/> is <c>true</c>.
    /// This property is required when archiving is enabled.
    /// </summary>
    public string? ArchiveTableName { get; set; } = null!;

    /// <summary>
    /// Indicates whether the application should attempt to create the specified 
    /// database if it doesn't already exist.
    /// </summary>
    public bool CreateDatabaseIfNotExists { get; set; }

    /// <summary>
    /// Indicates whether the application should attempt to create the specified 
    /// schema if it doesn't already exist.
    /// </summary>
    public bool CreateSchemaIfNotExists { get; set; }

    /// <summary>
    /// Gets the fully qualified table name in the format <c>SchemaName.TableName</c>.
    /// This combines the <see cref="SchemaName"/> and <see cref="TableName"/> 
    /// properties into a single string suitable for use in SQL queries.
    /// </summary>
    public string FullTableName => SchemaName + "." + TableName;

    /// <summary>
    /// Gets the fully qualified table name for archived messages in the format 
    /// <c>SchemaName.ArchiveTableName</c>.
    /// This combines the <see cref="SchemaName"/> and <see cref="ArchiveTableName"/> 
    /// properties into a single string suitable for use in SQL queries.
    /// This will be <c>null</c> if <see cref="ArchiveTableName"/> is not set or 
    /// is empty.
    /// </summary>
    public string? ArchiveFullTableName
    {
        get
        {
            return string.IsNullOrWhiteSpace(ArchiveTableName)
                ? null
                : SchemaName + "." + ArchiveTableName;
        }
    }
}
