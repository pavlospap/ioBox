using System.Data;

namespace IOBox.Persistence;

/// <summary>
/// Provides an abstraction for creating database connections based on 
/// configuration entries defined in <see cref="Options.DbOptions"/>.
/// </summary>
public interface IDbContext
{
    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> using the 
    /// <see cref="Options.DbOptions.ConnectionString"/> from the
    /// <see cref="Options.DbOptions"/> identified by the specified 
    /// <paramref name="ioName"/>.
    /// </summary>
    /// <param name="ioName">
    /// The inbox/outbox name used to resolve a corresponding 
    /// <see cref="Options.DbOptions"/> 
    /// configuration.
    /// </param>
    /// <returns>A new <see cref="IDbConnection"/> instance.</returns>
    IDbConnection CreateConnection(string ioName);

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> using the 
    /// <see cref="Options.DbOptions.DefaultConnectionString"/> from the
    /// <see cref="Options.DbOptions"/> identified by the specified 
    /// <paramref name="ioName"/>.
    /// This connection is used for administrative operations such as creating 
    /// the database if database creation is enabled.
    /// </summary>
    /// <param name="ioName">
    /// The inbox/outbox name used to resolve a corresponding 
    /// <see cref="Options.DbOptions"/> 
    /// configuration.
    /// </param>
    /// <returns>A new <see cref="IDbConnection"/> instance.</returns>
    IDbConnection CreateDefaultConnection(string ioName);
}
