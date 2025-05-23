using System.Data;

using IOBox.Persistence;
using IOBox.Persistence.Options;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace IOBox.SqlServer;

class SqlServerDbContext(
    IOptionsMonitor<DbOptions> dbOptionsMonitor) : IDbContext
{
    public IDbConnection CreateConnection(string ioName) =>
        new SqlConnection(dbOptionsMonitor.Get(ioName).ConnectionString);

    public IDbConnection CreateDefaultConnection(string ioName) =>
        new SqlConnection(dbOptionsMonitor.Get(ioName).DefaultConnectionString);
}
