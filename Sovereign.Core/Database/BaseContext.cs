using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Microsoft.EntityFrameworkCore;

namespace Sovereign.Core.Database;

public enum DatabaseConnectMode
{
    ReadWriteCreate,
    ReadWrite,
    ReadOnly,
    Memory,
}

public class BaseContext : DbContext
{
    /// <summary>
    /// SQL statement to run when checking if the migration history table exists.
    /// </summary>
    public const string CheckMigrationHistorySql = "SELECT MigrationId FROM __EFMigrationsHistory;";

    /// <summary>
    /// SQL statement to run when creating the database for migrates.
    /// </summary>
    public const string CreateMigrationHistorySql = "CREATE TABLE __EFMigrationsHistory (MigrationId TEXT PRIMARY KEY, ProductVersion TEXT NOT NULL);";
    
    /// <summary>
    /// SQL statement to check for a migration being already applied.
    /// </summary>
    public const string FindAppliedMigrationSql = "SELECT MigrationId FROM __EFMigrationsHistory WHERE MigrationId = {0};";

    /// <summary>
    /// SQL statement to add a migration as applied.
    /// </summary>
    public const string MigrationAppliedSql = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({0}, {1});";
    
    /// <summary>
    /// Name of the database.
    /// </summary>
    private readonly string? _databaseName;
    
    /// <summary>
    /// Custom path of the file.
    /// </summary>
    private readonly string? _filePath;

    /// <summary>
    /// Mode for connecting to the database.
    /// </summary>
    private readonly DatabaseConnectMode _connectMode;

    /// <summary>
    /// Creates a base SQLite context.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="filePath">File path to use for the context.</param>
    /// <param name="connectMode">Mode for connecting to the database.</param>
    public BaseContext(string databaseName, string? filePath = null, DatabaseConnectMode connectMode = DatabaseConnectMode.ReadWriteCreate)
    {
        this._databaseName = databaseName;
        this._filePath = filePath;
        this._connectMode = connectMode;
    }
    
    /// <summary>
    /// Configures the database.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var filePath = $"{this._databaseName}.sqlite";
        var databaseDirectoryLocation = Environment.GetEnvironmentVariable("DATABASE_DIRECTORY_LOCATION");
        if (databaseDirectoryLocation != null)
        {
            filePath = Path.Combine(databaseDirectoryLocation, filePath);
        }
        optionsBuilder.UseSqlite($"Data Source=\"{this._filePath ?? filePath}\";Mode={this._connectMode}");
    }

    /// <summary>
    /// Migrates the database to the latest schema.
    /// </summary>
    public async Task MigrateAsync()
    {
        // Prepare the initial table.
        Logger.Debug("Updating database schema.");
        await using var transaction = await this.Database.BeginTransactionAsync();
        try
        {
            await this.Database.ExecuteSqlAsync(FormattableStringFactory.Create(CheckMigrationHistorySql));
        }
        catch (Exception)
        {
            Logger.Debug("Creating initial migration history schema.");
            await this.Database.ExecuteSqlAsync(FormattableStringFactory.Create(CreateMigrationHistorySql));
        }
        
        // Apply any outstanding migrations.
        var assembly = this.GetType().Assembly;
        var resources = assembly.GetManifestResourceNames().Where(name => Regex.Match(name, $"{this._databaseName}..+.sql").Success).ToList();
        resources.Sort();
        foreach (var resourceName in resources)
        {
            // Ignore the migration if it was already applied.
            var migrateName = Regex.Match(resourceName, $"{this._databaseName}.(.+).sql").Groups[1].Value;
            var completedMigrates =  await this.Database.SqlQuery<string>(FormattableStringFactory.Create(FindAppliedMigrationSql, new object[] {migrateName})).ToListAsync();
            if (completedMigrates.Count > 0)
            {
                Logger.Trace($"Migration {migrateName} already applied.");
                continue;
            }
            
            // Apply the migration.
            Logger.Trace($"Applying migration {migrateName}.");
            await using var migrateStream = assembly.GetManifestResourceStream(resourceName);
            using var migrateStreamReader = new StreamReader(migrateStream!);
            await this.Database.ExecuteSqlAsync(FormattableStringFactory.Create(await migrateStreamReader.ReadToEndAsync()));
            await this.Database.ExecuteSqlAsync(FormattableStringFactory.Create(MigrationAppliedSql, new object[] {migrateName, "Sovereign"}));
        }
        
        // Commit the schema changes.
        await transaction.CommitAsync();
    }
}