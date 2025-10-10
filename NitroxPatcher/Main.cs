extern alias JB;
global using NitroxModel.Logger;
global using static NitroxClient.Extensions.NitroxEntityExtensions;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JB::JetBrains.Annotations;
using NitroxModel_Subnautica.Logger;
using UnityEngine;

namespace NitroxPatcher;

public static class Main
{
    /// <summary>
    ///     Lazily (i.e. when called, unlike immediately on class load) gets the path to the Nitrox Launcher folder.
    ///     This path can be anywhere on the system because it's placed somewhere the user likes.
    /// </summary>
    private static readonly Lazy<string> nitroxLauncherDir = new(() =>
    {
        // Get path from command args.
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            string path = (args[i], args[i + 1]) switch
            {
                ("--nitrox", { } value) when Directory.Exists(value) => value,
                _ => null
            };
            if (path is not null)
            {
                return Path.GetFullPath(path);
            }
        }

        // Get path from environment variable.
        string envPath = Environment.GetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY, EnvironmentVariableTarget.Process);
        if (Directory.Exists(envPath))
        {
            return envPath;
        }

        return null;
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

        Console.WriteLine("Checking if Nitrox should run...");
        if (!Directory.Exists(Environment.GetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY)))
        {
            Environment.SetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY, nitroxLauncherDir.Value, EnvironmentVariableTarget.Process);
        }
        if (!Directory.Exists(Environment.GetEnvironmentVariable(NitroxUser.LAUNCHER_PATH_ENV_KEY)))
        {
            Console.WriteLine("Nitrox will not load because launcher path was not provided.");
            return;
        }

        Console.WriteLine("Now initializing Nitrox...");
        InitWithDependencies();
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

        Log.Info($"Using Nitrox {NitroxEnvironment.VersionInfo} built on {NitroxEnvironment.BuildDate:F}");
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
            Console.Write($"Did not find '{dllFileName}' at '{dllPath}'");
            dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, dllFileName);
            Console.WriteLine($", looking at {dllPath}");
        }

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"Nitrox dll missing: {dllPath}");
        }

        return Assembly.LoadFile(dllPath);
    }
}
