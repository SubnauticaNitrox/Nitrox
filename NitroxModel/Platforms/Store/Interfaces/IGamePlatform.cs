using System.Threading.Tasks;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Platforms.Store.Interfaces
{
    public interface IGamePlatform
    {
        /// <summary>
        ///     Tries to start the platform and waits for it to be ready to launch games. If it has already been started it will return true.
        /// </summary>
        /// <returns>Returns true if platform is running or has started successfully.</returns>
        public Task<ProcessEx> StartPlatformAsync();

        /// <summary>
        ///     Tries to find the executable of the platform or null if not found.
        /// </summary>
        /// <returns></returns>
        public string GetExeFile();

        /// <summary>
        ///     True if game directory originates from the game platform.
        /// </summary>
        /// <param name="gameDirectory">Directory to a game, usually where the exe file is.</param>
        /// <returns>True if the game platform owns this game.</returns>
        bool OwnsGame(string gameDirectory);

        string Name { get; }
    }
}
