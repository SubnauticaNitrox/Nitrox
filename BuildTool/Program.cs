using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Discovery;
using NitroxModel.Discovery.InstallationFinders;

namespace BuildTool
{
    /// <summary>
    ///     Entry point of the build automation project.
    ///     1. Search for Subnautica install.
    ///     2. Publicize the .NET dlls and persist for subsequent Nitrox builds.
    /// </summary>
    public static class Program
    {
        private static readonly Lazy<string> processDir =
            new(() => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory()));

        private static uint SteamAppId => !uint.TryParse(Environment.GetEnvironmentVariable("SteamAppId"), out uint id) ? SteamGameRegistryFinder.SUBNAUTICA_APP_ID : id;

        public static string ProcessDir => processDir.Value;

        public static string GeneratedOutputDir => Path.Combine(ProcessDir, "generated_files");

        public static async Task Main(string[] args)
        {
            if (SteamAppId <= 0) throw new Exception("SteamAppId environment variable must be set and be valid");

            Console.WriteLine($"Building mod for Steam game with id {SteamAppId}");
            GameInstallData game = await Task.Factory.StartNew(() => EnsureGame(SteamAppId)).ConfigureAwait(false);
            Console.WriteLine($"Found game at {game.InstallDir}");
            await Task.Factory.StartNew(() => EnsurePublicizedAssemblies(game)).ConfigureAwait(false);
        }

        private static GameInstallData EnsureGame(uint steamAppId)
        {
            static string ValidateUnityGame(GameInstallData game)
            {
                if (!File.Exists(Path.Combine(game.InstallDir, "UnityPlayer.dll")))
                {
                    return "Steam game is not a Unity game.";
                }
                if (!Directory.Exists(game.ManagedDllsDir))
                {
                    throw new Exception($"Invalid Unity managed DLLs directory: {game.ManagedDllsDir}");
                }
                return null;
            }

            string cacheFile = Path.Combine(GeneratedOutputDir, "game.props");
            GameInstallData game = GameInstallData.TryFrom(cacheFile);
            if (game == null || ValidateUnityGame(game) != null)
            {
                game = new GameInstallData(GameInstallationFinder.Instance.FindGame());
            }

            string error = ValidateUnityGame(game);
            if (error != null)
            {
                throw new Exception(error);
            }

            game.TrySave(cacheFile);
            return game;
        }

        private static void EnsurePublicizedAssemblies(GameInstallData game)
        {
            if (Directory.Exists(Path.Combine(GeneratedOutputDir, "publicized_assemblies")))
            {
                Console.WriteLine("Assemblies are already publicized.");
                return;
            }

            string[] dllsToPublicize = Directory.GetFiles(game.ManagedDllsDir, "Assembly-*.dll");
            foreach (string publicizedDll in Publicizer.Execute(dllsToPublicize,
                                                                "_publicized",
                                                                Path.Combine(GeneratedOutputDir, "publicized_assemblies")))
            {
                Console.WriteLine($"Publicized dll: {publicizedDll}");
            }
        }
    }
}
