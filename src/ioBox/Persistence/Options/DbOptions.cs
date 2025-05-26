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
    /// If specified, archived messages will be stored in this table as long as 
    /// <see cref="Workers.Archive.Options.ArchiveOptions.Enabled"/> is <c>true</c>.
    /// This property becomes mandatory when archiving is enabled.
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
}
