using Dapper;

using IOBox.Persistence;
using IOBox.Persistence.Options;
using IOBox.Workers.Archive.Options;

using Microsoft.Extensions.Options;

namespace IOBox.SqlServer;

class SqlServerDbMigrator(
    IDbContext dbContext,
    IOptionsMonitor<DbOptions> dbOptionsMonitor,
    IOptionsMonitor<ArchiveOptions> archiveOptionsMonitor) : IDbMigrator
{
    public void MigrateDb(string ioName)
    {
        var dbOptions = dbOptionsMonitor.Get(ioName);

        if (dbOptions.CreateDatabaseIfNotExists)
        {
            CreateDb(dbOptions.DatabaseName!, ioName);
        }

        if (dbOptions.CreateSchemaIfNotExists)
        {
            CreateSchema(dbOptions.SchemaName, ioName);
        }

        CreateTable(
            dbOptions.SchemaName,
            dbOptions.TableName,
            dbOptions.FullTableName,
            ioName);

        var archiveOptions = archiveOptionsMonitor.Get(ioName);

        if (archiveOptions.Enabled)
        {
            CreateTable(
                dbOptions.SchemaName,
                dbOptions.ArchiveTableName!,
                dbOptions.ArchiveFullTableName!,
                ioName);
        }
    }

    void CreateDb(string name, string ioName)
    {
        var sql = "SELECT 1 FROM sys.databases WHERE name = @name;";

        using var connection = dbContext.CreateDefaultConnection(ioName);

        var records = connection.Query(sql, new { name });

        if (!records.Any())
        {
            connection.Execute($"CREATE DATABASE {name};");
        }
    }

    void CreateSchema(string name, string ioName)
    {
        var sql = "SELECT 1 FROM sys.schemas WHERE name = @name;";

        using var connection = dbContext.CreateConnection(ioName);

        var records = connection.Query(sql, new { name });

        if (!records.Any())
        {
            connection.Execute($"CREATE SCHEMA {name};");
        }
    }

    void CreateTable(
        string schemaName,
        string tableName,
        string fullTableName,
        string ioName)
    {
        var sql =
            "SELECT 1 FROM sys.tables " +
            "WHERE name = @tableName AND schema_id = SCHEMA_ID(@schemaName);";

        using var connection = dbContext.CreateConnection(ioName);

        var records = connection.Query(sql, new { tableName, schemaName });

        if (records.Any())
        {
            return;
        }

        sql = $@"
            CREATE TABLE {fullTableName} (
              Id int IDENTITY NOT NULL,
              MessageId nvarchar(50) NOT NULL,
              Message nvarchar(max) NOT NULL,
              ContextInfo nvarchar(max) NULL,
              Status tinyint NOT NULL,
              Retries int NOT NULL,
              Error nvarchar(max) NULL,
              ReceivedAt datetimeoffset NOT NULL,
              LockedAt datetimeoffset NULL,
              ProcessedAt datetimeoffset NULL,
              FailedAt datetimeoffset NULL,
              ExpiredAt datetimeoffset NULL
            CONSTRAINT PK_{tableName} PRIMARY KEY CLUSTERED (Id));

            CREATE UNIQUE INDEX UX_{tableName}_MessageId 
                ON {fullTableName}(MessageId);

            CREATE INDEX IX_{tableName}_ReceivedAt 
                ON {fullTableName} (ReceivedAt)
                WHERE (Status = 1);

            CREATE INDEX IX_{tableName}_LockedAt 
                ON {fullTableName} (LockedAt)
                WHERE (Status = 2);

            CREATE INDEX IX_{tableName}_ProcessedAt 
                ON {fullTableName} (ProcessedAt)
                WHERE (Status = 3);

            CREATE INDEX IX_{tableName}_FailedAt 
                ON {fullTableName} (FailedAt)
                INCLUDE (Retries)
                WHERE (Status = 4);

            CREATE INDEX IX_{tableName}_ExpiredAt 
                ON {fullTableName} (ExpiredAt)
                WHERE (Status = 5);";

        connection.Execute(sql);
    }
}
