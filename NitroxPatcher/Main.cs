global using NitroxModel.Logger;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Win32;
using NitroxModel.Helper;
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
            if (args[i].Equals("-nitrox", StringComparison.OrdinalIgnoreCase) && Directory.Exists(args[i + 1]))
            {
                return Path.GetFullPath(args[i + 1]);
            }
        }

        // Get path from environment variable.
        string envPath = Environment.GetEnvironmentVariable("NITROX_LAUNCHER_PATH");
        if (Directory.Exists(envPath))
        {
            return envPath;
        }

        // Get path from windows registry.
        using RegistryKey nitroxRegKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Nitrox");
        if (nitroxRegKey == null)
        {
            return null;
        }

        string path = nitroxRegKey.GetValue("LauncherPath") as string;
        return Directory.Exists(path) ? path : null;
    });

    private static readonly char[] newLineChars = Environment.NewLine.ToCharArray();

    /// <summary>
    ///     Entrypoint of Nitrox. Code in this method cannot use other dependencies (DLLs) without crashing
    ///     due to <see cref="AppDomain.AssemblyResolve" /> not being called.
    ///     Use the <see cref="Init" /> method or later before using dependency code.
    /// </summary>
    [UsedImplicitly]
    public static void Execute()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

        if (nitroxLauncherDir.Value == null)
        {
            Console.WriteLine("Nitrox will not load because launcher path was not provided.");
            return;
        }

        Environment.SetEnvironmentVariable("NITROX_LAUNCHER_PATH", nitroxLauncherDir.Value);

        Init();
    }

    /// <summary>
    ///     This method must not be inlined since the AppDomain dependency resolve will be triggered when the JIT compiles this method. If it's inlined it will cause dependencies to
    ///     resolve in <see cref="Execute" /> *before* the dependency resolve listener is applied.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Init()
    {
        Log.Setup(inGameLogger: new SubnauticaInGameLogger(), useConsoleLogging: false);
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
                        toWrite += Environment.NewLine + stackTrace;
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

        Log.Info($"Using Nitrox {NitroxEnvironment.ReleasePhase} V{NitroxEnvironment.Version} built on {NitroxEnvironment.BuildDate}");
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
    private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        string dllFileName = args.Name.Split(',')[0];
        if (!dllFileName.EndsWith(".dll"))
        {
            dllFileName += ".dll";
        }

        // Load DLLs where Nitrox launcher is first, if not found, use Subnautica's DLLs.
        string dllPath = Path.Combine(nitroxLauncherDir.Value, "lib", dllFileName);
        if (!File.Exists(dllPath))
        {
            dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dllFileName);
        }

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"Nitrox dll missing: {dllPath}");
        }
        return Assembly.LoadFile(dllPath);
    }
}
