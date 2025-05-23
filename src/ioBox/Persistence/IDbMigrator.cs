namespace IOBox.Persistence;

/// <summary>
/// Defines a contract for applying database schema migrations.
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Applies schema migrations for the database.
    /// </summary>
    /// <param name="ioName">
    /// The inbox/outbox name used to resolve a corresponding 
    /// <see cref="Options.DbOptions"/> 
    /// configuration.
    /// </param>
    void MigrateDb(string ioName);
}
