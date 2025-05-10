using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Models.Configuration;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Initializes the database and provides access to it.
/// </summary>
internal sealed class DatabaseService(IDbContextFactory<WorldDbContext> dbContextFactory, IOptions<SqliteOptions> optionsProvider, IHostEnvironment hostEnvironment, ILogger<DatabaseService> logger) : IHostedLifecycleService
{
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly IHostEnvironment hostEnvironment = hostEnvironment;
    private readonly ILogger<DatabaseService> logger = logger;
    private readonly IOptions<SqliteOptions> optionsProvider = optionsProvider;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        // Ensure database is up-to-date.
        await using WorldDbContext db = await GetDbContextAsync();
        if (hostEnvironment.IsDevelopment())
        {
            try
            {
                await db.Database.EnsureDeletedAsync(cancellationToken);
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something is blocking the SQLite database. Check that you do not have it open in your IDE or other database viewer.");
                throw;
            }
        }
        else
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        await ExecuteOptionsAsPragma(db, optionsProvider.Value);
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<WorldDbContext> GetDbContextAsync() => await dbContextFactory.CreateDbContextAsync();

    private async Task ExecuteCommand(WorldDbContext db, string command)
    {
        try
        {
            logger.LogDebug("Executing database command \"{Command}\"", command);
            await using DbConnection connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            await using DbCommand commandObj = connection.CreateCommand();
            commandObj.CommandText = command;
            await commandObj.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to execute command {Command}: {Error}", command, ex.Message);
        }
    }

    private async Task ExecuteOptionsAsPragma(WorldDbContext db, SqliteOptions options)
    {
        StringBuilder pragmaBuilder = new();
        foreach (PropertyInfo property in options.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            ConfigurationKeyNameAttribute configKeyAttr = property.GetCustomAttribute<ConfigurationKeyNameAttribute>();
            string pragmaKey = configKeyAttr?.Name;
            if (string.IsNullOrWhiteSpace(pragmaKey))
            {
                continue;
            }
            string pragmaValue = property.GetValue(options)?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(pragmaValue))
            {
                logger.ZLogWarning($"Pragma {pragmaKey:@Key} has no value");
                continue;
            }

            pragmaBuilder
                .Append("PRAGMA ")
                .Append(pragmaKey)
                .Append('=')
                .Append(pragmaValue)
                .Append(';');
        }
        if (pragmaBuilder.Length > 0)
        {
            await ExecuteCommand(db, pragmaBuilder.ToString());
        }
    }
}
