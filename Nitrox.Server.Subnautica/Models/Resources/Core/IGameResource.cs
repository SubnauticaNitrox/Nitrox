namespace Nitrox.Server.Subnautica.Models.Resources.Core;

/// <summary>
///     Implementors of this interface are parsing game files and loading its data into memory.
/// </summary>
internal interface IGameResource
{
    Task LoadAsync(CancellationToken cancellationToken);
    Task CleanupAsync();
}
