using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NitroxModel.Discovery;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.BuildTool
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

        public static string ProcessDir => processDir.Value;

        public static string GeneratedOutputDir => Path.Combine(ProcessDir, "generated_files");

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(eventArgs.ExceptionObject);
                Console.ResetColor();

                Exit((eventArgs.ExceptionObject as Exception)?.HResult ?? 1);
            };

            ConfiguredTaskAwaitable litenetlibRepo = Task.Run(() => EnsureGitDependency("https://github.com/RevenantX/LiteNetLib.git", "LiteNetLib")).ConfigureAwait(false);
            GameInstallData game = await Task.Factory.StartNew(EnsureGame).ConfigureAwait(false);
            Console.WriteLine($"Found game at {game.InstallDir}");
            await Task.Factory.StartNew(() => EnsurePublicizedAssemblies(game)).ConfigureAwait(false);
            await litenetlibRepo;

            Exit();
        }

        private static void Exit(int exitCode = 0)
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey(true);
            Environment.Exit(exitCode);
        }

        private static GameInstallData EnsureGame()
        {
            static bool ValidateUnityGame(GameInstallData game, out string error)
            {
                if (!File.Exists(Path.Combine(game.InstallDir, "UnityPlayer.dll")))
                {
                    error = $"Game at: '{game.InstallDir}' is not a Unity game";
                    return false;
                }
                if (!Directory.Exists(game.ManagedDllsDir))
                {
                    error = $"Invalid Unity managed DLLs directory: {game.ManagedDllsDir}";
                    return false;
                }

                error = null;
                return true;
            }

            string cacheFile = Path.Combine(GeneratedOutputDir, "game.props");
            if (GameInstallData.TryFrom(cacheFile, out GameInstallData game) && !ValidateUnityGame(game, out string error))
            {
                throw new Exception(error);
            }

            game ??= new GameInstallData(GameInstallationFinder.Instance.FindGame());
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
                                                                "",
                                                                Path.Combine(GeneratedOutputDir, "publicized_assemblies")))
            {
                Console.WriteLine($"Publicized dll: {publicizedDll}");
            }
        }

        private static void EnsureGitDependency(string repoUrl, string projectName)
        {
            if (File.Exists($@"repositories\{projectName}\{projectName}\bin\Release\{projectName}.dll"))
            {
                Console.WriteLine($"Repository: {repoUrl} already downloaded and built");
                return;
            }
            
            FileSystem.Instance.ShellExecute($"git clone \"{repoUrl}\" && dotnet build {projectName} --configuration Release", "repositories");
            Console.WriteLine($"Project {projectName} downloaded and built");
        }
    }
}
