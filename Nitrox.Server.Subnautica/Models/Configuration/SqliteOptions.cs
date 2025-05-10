using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Nitrox.Server.Subnautica.Models.Configuration;

/// <summary>
///     Options available to configure the Sqlite database. See
///     <a href="https://sqlite.org/pragma.html">Sqlite PRAGMA documentation</a> for more information.
/// </summary>
internal sealed partial class SqliteOptions
{
    /// <inheritdoc cref="SqliteTempStore" />
    [Required]
    [ConfigurationKeyName("temp_store")]
    public SqliteTempStore TempStore { get; set; } = SqliteTempStore.MEMORY;

    [Required]
    [ConfigurationKeyName("cache_size")]
    public int CacheSize { get; set; } = -32000;

    [Required]
    [ConfigurationKeyName("page_size")]
    public int PageSize { get; set; } = -32768;

    /// <inheritdoc cref="Sync" />
    [Required]
    [ConfigurationKeyName("synchronous")]
    public Sync Synchronous { get; set; } = Sync.OFF;

    /// <inheritdoc cref="LockMode" />
    [Required]
    [ConfigurationKeyName("locking_mode")]
    public LockMode LockingMode { get; set; } = LockMode.EXCLUSIVE;

    /// <inheritdoc cref="JournalingMode" />
    [Required]
    [ConfigurationKeyName("journal_mode")]
    public JournalingMode JournalMode { get; set; } = JournalingMode.WAL;

    /// <summary>
    ///     <a href="https://sqlite.org/pragma.html#pragma_journal_mode">Sqlite PRAGMA <c>journal_mode</c></a>
    /// </summary>
    public enum JournalingMode
    {
        DELETE,
        TRUNCATE,
        PERSIST,
        MEMORY,
        WAL,
        OFF
    }

    /// <summary>
    ///     <a href="https://sqlite.org/pragma.html#pragma_locking_mode">Sqlite PRAGMA <c>locking_mode</c></a>
    /// </summary>
    public enum LockMode
    {
        NORMAL,
        EXCLUSIVE
    }

    /// <summary>
    ///     <a href="https://sqlite.org/pragma.html#pragma_temp_store">Sqlite PRAGMA <c>temp_store</c></a>
    /// </summary>
    public enum SqliteTempStore
    {
        DEFAULT,
        FILE,
        MEMORY
    }

    /// <summary>
    ///     <a href="https://sqlite.org/pragma.html#pragma_synchronous">Sqlite PRAGMA <c>synchronous</c></a>
    /// </summary>
    public enum Sync
    {
        OFF,
        NORMAL,
        FULL,
        EXTRA
    }

    [OptionsValidator]
    internal partial class Validator : IValidateOptions<SqliteOptions>;
}
