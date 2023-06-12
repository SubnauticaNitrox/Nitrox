﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Discovery;
using NitroxModel.Helper;

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

        private const int LEGACY_BRANCH_SUBNAUTICA_VERSION = 68598;

        public static async Task Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                LogError(eventArgs.ExceptionObject.ToString());
                Exit((eventArgs.ExceptionObject as Exception)?.HResult ?? 1);
            };

            GameInstallData game = await Task.Run(EnsureGame);
            Console.WriteLine($"Found game at {game.InstallDir}");
            AbortIfInvalidGameVersion(game);
            await EnsurePublicizedAssembliesAsync(game);

            Exit();
        }

        private static void Exit(int exitCode = 0)
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey(true);
            Environment.Exit(exitCode);
        }

        private static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void AbortIfInvalidGameVersion(GameInstallData game)
        {
            string gameVersionFile = Path.Combine(game.InstallDir, "Subnautica_Data", "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
            if (!File.Exists(gameVersionFile))
            {
                return;
            }
            if (!int.TryParse(File.ReadAllText(gameVersionFile), out int version))
            {
                return;
            }
            if (version == -1)
            {
                return;
            }
            if (version > LEGACY_BRANCH_SUBNAUTICA_VERSION)
            {
                return;
            }

            LogError($"""
                        Game version is {version}, which is not supported by Nitrox.
                        Please update your game to the latest version.
                        """);
            Exit(2);
        }

        private static GameInstallData EnsureGame()
        {
            static bool ValidateUnityGame(GameInstallData game, out string error)
            {
                if (string.IsNullOrWhiteSpace(game.InstallDir))
                {
                    error = $"Path to game is not found: '{game.InstallDir}'";
                    return false;
                }
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
            if (GameInstallData.TryFrom(cacheFile, out GameInstallData game))
            {
                // Retry if the saved path is invalid
                if (!Directory.Exists(game.InstallDir))
                {
                    game = new GameInstallData(NitroxUser.GamePath);
                }

                if (!ValidateUnityGame(game, out string error))
                {
                    throw new Exception(error);
                }
            }

            game ??= new GameInstallData(NitroxUser.GamePath);
            game.TrySave(cacheFile);
            return game;
        }

        private static async Task EnsurePublicizedAssembliesAsync(GameInstallData game)
        {
            static void LogReceived(object sender, string message) => Console.WriteLine(message);

            if (Directory.Exists(Path.Combine(GeneratedOutputDir, "publicized_assemblies")))
            {
                Console.WriteLine("Assemblies are already publicized.");
                return;
            }

            string[] dllsToPublicize = Directory.GetFiles(game.ManagedDllsDir, "Assembly-*.dll");
            Publicizer.LogReceived += LogReceived;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                await Publicizer.PublicizeAsync(dllsToPublicize, "", Path.Combine(GeneratedOutputDir, "publicized_assemblies"));
            }
            finally
            {
                sw.Stop();
                Publicizer.LogReceived -= LogReceived;
            }
            Console.WriteLine($"Publicized {dllsToPublicize.Length} DLL(s) in {Math.Round(sw.Elapsed.TotalSeconds, 2)}s");
        }
    }
}
