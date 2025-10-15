using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Model.Platforms.Store.Interfaces;

public interface IGamePlatform
{
    string Name { get; }

    Platform Platform { get; }

    /// <summary>
    ///     True if game directory originates from the game platform.
    /// </summary>
    /// <param name="gameDirectory">Directory to a game, usually where the exe file is.</param>
    /// <returns>Returns true if the game platform owns this game.</returns>
    bool OwnsGame(string gameDirectory);
}
