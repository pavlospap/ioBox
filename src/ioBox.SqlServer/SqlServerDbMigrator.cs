using Dapper;

using IOBox.Persistence;
using IOBox.Persistence.Options;

using Microsoft.Extensions.Options;

namespace IOBox.SqlServer;

class SqlServerDbMigrator(
    IDbContext dbContext,
    IOptionsMonitor<DbOptions> dbOptionsMonitor) : IDbMigrator
{
    public void MigrateDb(string ioName)
    {
        CreateDb(ioName);

        CreateSchema(ioName);

        CreateTable(ioName);
    }

    void CreateDb(string ioName)
    {
        var options = dbOptionsMonitor.Get(ioName);

        if (!options.CreateDatabaseIfNotExists)
        {
            return;
        }

        var name = options.DatabaseName;

        var sql = "SELECT 1 FROM sys.databases WHERE name = @name;";

        using var connection = dbContext.CreateDefaultConnection(ioName);

        var records = connection.Query(sql, new { name });

        if (!records.Any())
        {
            connection.Execute($"CREATE DATABASE {name};");
        }
    }

    void CreateSchema(string ioName)
    {
        var options = dbOptionsMonitor.Get(ioName);

        if (!options.CreateSchemaIfNotExists)
        {
            return;
        }

        var name = options.SchemaName;

        var sql = "SELECT 1 FROM sys.schemas WHERE name = @name;";

        using var connection = dbContext.CreateConnection(ioName);

        var records = connection.Query(sql, new { name });

        if (!records.Any())
        {
            connection.Execute($"CREATE SCHEMA {name};");
        }
    }

    void CreateTable(string ioName)
    {
        var options = dbOptionsMonitor.Get(ioName);

        var tableName = options.TableName;

        var schemaName = options.SchemaName;

        var sql =
            "SELECT 1 FROM sys.tables " +
            "WHERE name = @tableName AND schema_id = SCHEMA_ID(@schemaName);";

        using var connection = dbContext.CreateConnection(ioName);

        var records = connection.Query(sql, new { tableName, schemaName });

        if (records.Any())
        {
            return;
        }

        var fullTableName = schemaName + "." + tableName;

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
