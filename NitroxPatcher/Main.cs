extern alias JB;
global using Nitrox.Model.Logger;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JB::JetBrains.Annotations;
using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.Logger;
using NitroxPatcher.Helper;
using UnityEngine;

namespace NitroxPatcher;

public static class Main
{
    /// <summary>
    ///     Lazily (i.e. when called, unlike immediately on class load) gets the path to the Nitrox Launcher folder.
    ///     This path can be anywhere on the system because it's placed somewhere the user likes. Doesn't test if the path exists.
    /// </summary>
    private static readonly Lazy<string> nitroxLauncherDir = new(() =>
    {
        // Get path from environment variable.
        string envPath = Environment.GetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrEmpty(envPath))
        {
            return envPath;
        }

        // Get path from command args.
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            string path = (args[i], args[i + 1]) switch
            {
                ("--nitrox", { } value) when !string.IsNullOrEmpty(value) => Path.GetFullPath(value),
                _ => null
            };
            if (!string.IsNullOrEmpty(path))
            {
                Environment.SetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY, path, EnvironmentVariableTarget.Process);
                return path;
            }
        }

        return string.Empty;
    });

    private static readonly char[] newLineChars = Environment.NewLine.ToCharArray();
    private static bool initialized;

    /// <summary>
    ///     Entrypoint of Nitrox. Code in this method cannot use other dependencies (DLLs) without crashing
    ///     due to <see cref="AppDomain.AssemblyResolve" /> not being called.
    ///     Use the <see cref="InitWithDependencies" /> method or later before using dependency code.
    /// </summary>
    [UsedImplicitly]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Execute()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

        Console.WriteLine("[Nitrox] [INFO] Checking if it should run...");
        if (!ShouldRun())
        {
            return;
        }

        Console.WriteLine("[Nitrox] [INFO] Now initializing...");
        InitWithDependencies();

        static bool ShouldRun()
        {
            try
            {
                if (string.IsNullOrEmpty(nitroxLauncherDir.Value))
                {
                    Console.WriteLine($"[Nitrox] [WARNING] will not load because launcher path was not provided: '{nitroxLauncherDir.Value}'");
                    return false;
                }
                if (!Directory.Exists(nitroxLauncherDir.Value))
                {
                    Console.WriteLine($"[Nitrox] [WARNING] will not load because launcher path does not exist or is inaccessible: '{nitroxLauncherDir.Value}'");
                    string lastPath = "";
                    string? currentFolder = nitroxLauncherDir.Value;
                    do
                    {
                        if (!Directory.Exists(currentFolder))
                        {
                            lastPath = currentFolder;
                        }
                        else
                        {
                            Console.WriteLine($"[Nitrox] [WARNING] Path '{lastPath}' is inaccessible, however '{currentFolder}' is.");
                            Console.WriteLine("[Nitrox] [WARNING] To fix, please move Nitrox closer to the game so that the game can access Nitrox files.");
                            return false;
                        }
                        currentFolder = Path.GetDirectoryName(currentFolder);
                    } while (currentFolder != null);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Nitrox] [ERROR] {ex}");
                return false;
            }
        }
    }

    /// <summary>
    ///     This method must not be inlined since the AppDomain dependency resolve will be triggered when the JIT compiles this method. If it's inlined it will cause dependencies to
    ///     resolve in <see cref="Execute" /> *before* the dependency resolve listener is applied.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void InitWithDependencies()
    {
        Log.Setup(gameLogger: new SubnauticaInGameLogger(), useConsoleLogging: false);

        // Capture unity errors to be logged by our logging framework.
        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    string toWrite = condition;
                    if (!string.IsNullOrWhiteSpace(stackTrace))
                    {
                        toWrite += $"{Environment.NewLine}{stackTrace}";
                    }
                    Log.ErrorUnity(toWrite.Trim(newLineChars));
                    break;
                case LogType.Warning:
                case LogType.Log:
                case LogType.Assert:
                    // These logs from Unity spam too much uninteresting stuff
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        };

        Log.Info($"[Nitrox] [INFO] Using {NitroxEnvironment.VersionInfo} built on {NitroxEnvironment.BuildDate:F}");
        Log.Info($"[Nitrox] [INFO] Game version: {Application.version}");
        // Log if other mods are loaded
        Task.Run(async () =>
        {
            if (SupportHelper.GetSummaryOfOtherMods() is { } otherModsSummary)
            {
                Log.Warn(otherModsSummary);
                return;
            }
            // ... no other mods right now, we try again after a wait period.
            await Task.Delay(TimeSpan.FromSeconds(2));
            otherModsSummary = SupportHelper.GetSummaryOfOtherMods();
            if (otherModsSummary != null)
            {
                Log.Warn(otherModsSummary);
            }
        }).ContinueWithHandleError();
        try
        {
            Patcher.Initialize();
        }
        catch (Exception ex)
        {
            // Placeholder for popup gui
            Log.Error(ex, "Unhandled exception occurred while initializing Nitrox:");
        }
    }

    /// <summary>
    /// Nitrox DLL location resolver.
    /// <p/>
    /// Required to load the files from the Nitrox Launcher subfolder which would otherwise not be found.
    /// </summary>
    private static Assembly? CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        string dllFileName = args.Name.Split(',')[0];
        if (!dllFileName.EndsWith(".dll"))
        {
            dllFileName += ".dll";
        }
        if (dllFileName.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Load DLLs where Nitrox launcher is first, if not found, use Subnautica's DLLs.
        string dllPath = Path.Combine(nitroxLauncherDir.Value, "lib", "net472", dllFileName);
        if (!File.Exists(dllPath))
        {
            Console.Write($"[Nitrox] [WARNING] Did not find '{dllFileName}' at '{dllPath}'");
            dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, dllFileName);
            Console.WriteLine($", looking at {dllPath}");
        }

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"[Nitrox] [WARNING] dll missing: {dllPath}");
        }

        return Assembly.LoadFile(dllPath);
    }
}
