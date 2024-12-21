using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Svg.Skia;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal static class Program
{
    internal const string CRASH_REPORT_FILE_NAME = "Nitrox.Launcher-crash.txt";
    internal static Func<Window> StartupWindowFactory;
    internal static InstantLaunchData InstantLaunch;
    internal static bool IsCrashReport;

    /// <summary>
    ///     If true, allows duplicate instances of the app to be active.
    /// </summary>
    internal static bool AllowInstances;

    internal static X11RenderingMode? PreferredRenderingMode;

    // Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code before AppMain is called
    // Things aren't initialized yet and stuff might break
    [STAThread]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

        LoadAvalonia(args);
    }

    internal static void HandleUnhandledException(Exception ex)
    {
        if (IsCrashReport)
        {
            Log.Error(ex, "Error while trying to show crash report");
            return;
        }

        Log.Error(ex, "!!!Nitrox Launcher Crash!!!");

        // Write crash report if we're not reporting one right now.
        try
        {
            string executableFilePath = NitroxUser.ExecutableFilePath ?? Environment.ProcessPath;
            string executableRoot = Path.GetDirectoryName(executableFilePath);
            if (executableFilePath != null && executableRoot != null)
            {
                string crashReportFile = Path.Combine(executableRoot, CRASH_REPORT_FILE_NAME);
                File.WriteAllText(crashReportFile, ex.ToString());
                string noExtension = Path.ChangeExtension(executableFilePath, null);
                if (File.Exists(noExtension))
                {
                    executableFilePath = noExtension;
                }
                using Process proc = ProcessUtils.StartProcessDetached(new ProcessStartInfo(executableFilePath, ["--crash-report"]));
            }
            else
            {
                Log.Error(ex, "Unable to get executable file path for writing crash report.");
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            Console.WriteLine(exception);
        }
        Environment.Exit(1);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        // https://github.com/wieslawsoltes/Svg.Skia?tab=readme-ov-file#avalonia-previewer
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);

        CultureManager.ConfigureCultureInfo();
        Log.Setup();

        // Handle command line arguments.
        ConsoleApp.ConsoleAppBuilder cliParser = ConsoleApp.Create();
        cliParser.Add("", (bool crashReport, X11RenderingMode? rendering = null, bool allowInstances = false) =>
        {
            IsCrashReport = crashReport;
            PreferredRenderingMode = rendering;
            AllowInstances = allowInstances;

            if (IsCrashReport)
            {
                string executableRootPath = Path.GetDirectoryName(Environment.ProcessPath ?? NitroxUser.ExecutableRootPath);
                if (executableRootPath != null)
                {
                    string crashReportContent = File.ReadAllText(Path.Combine(executableRootPath, CRASH_REPORT_FILE_NAME));
                    StartupWindowFactory = () => new CrashWindow { DataContext = new CrashWindowViewModel { Title = $"Nitrox {NitroxEnvironment.Version} - Crash Report", Message = crashReportContent } };
                }
            }
        });
        cliParser.Add("instantlaunch", ([SaveName] string save, [MinLength(1)] params string[] players) =>
        {
            InstantLaunch = new InstantLaunchData(save, players);
        });
        cliParser.Run(Environment.GetCommandLineArgs().Skip(1).ToArray());

        // Fallback to normal startup.
        if (StartupWindowFactory == null)
        {
            if (!AllowInstances)
            {
                CheckForRunningInstance();
            }
            ServiceProvider services = new ServiceCollection().AddAppServices().BuildServiceProvider();
            StartupWindowFactory = () => new MainWindow { DataContext = services.GetRequiredService<MainWindowViewModel>() };
        }

        AppBuilder builder = AppBuilder.Configure<App>()
                                       .UsePlatformDetect()
                                       .LogToTrace()
                                       .UseReactiveUI()
                                       .With(new SkiaOptions { UseOpacitySaveLayer = true });
        builder = WithRenderingMode(builder, PreferredRenderingMode);
        return builder;

        static AppBuilder WithRenderingMode(AppBuilder builder, X11RenderingMode? rendering)
        {
            if (rendering.HasValue)
            {
                return builder.With(new X11PlatformOptions { RenderingMode = [rendering.Value] });
            }
            // The Wayland+GPU is not supported by Avalonia, but Xwayland should work.
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
            {
                if (!ProcessEx.ProcessExists("Xwayland"))
                {
                    return builder.With(new X11PlatformOptions { RenderingMode = [X11RenderingMode.Software] });
                }
            }
            return builder;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LoadAvalonia(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            HandleUnhandledException(ex);
        }
    }

    private static void CheckForRunningInstance()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        try
        {
            using ProcessEx process = ProcessEx.GetFirstProcess("Nitrox.Launcher", process => process.Id != Environment.ProcessId && process.IsRunning);
            if (process is not null)
            {
                process.SetForegroundWindowAndRestore();
                Environment.Exit(0);
            }
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private static class AssemblyResolver
    {
        private static string currentExecutableDirectory;

        public static Assembly Handler(object sender, ResolveEventArgs args)
        {
            static Assembly ResolveFromLib(ReadOnlySpan<char> dllName)
            {
                dllName = dllName.Slice(0, dllName.IndexOf(','));
                if (!dllName.EndsWith(".dll"))
                {
                    dllName = string.Concat(dllName, ".dll");
                }

                if (dllName.EndsWith(".resources.dll"))
                {
                    return null;
                }

                string dllNameStr = dllName.ToString();

                string dllPath = Path.Combine(GetExecutableDirectory(), "lib", dllNameStr);
                if (!File.Exists(dllPath))
                {
                    dllPath = Path.Combine(GetExecutableDirectory(), dllNameStr);
                }

                try
                {
                    return Assembly.LoadFile(dllPath);
                }
                catch
                {
                    return null;
                }
            }

            Assembly assembly = ResolveFromLib(args.Name);
            if (assembly == null && !args.Name.Contains(".resources"))
            {
                assembly = Assembly.Load(args.Name);
            }

            return assembly;
        }

        private static string GetExecutableDirectory()
        {
            if (currentExecutableDirectory != null)
            {
                return currentExecutableDirectory;
            }
            string pathAttempt = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrWhiteSpace(pathAttempt))
            {
                using Process proc = Process.GetCurrentProcess();
                pathAttempt = proc.MainModule?.FileName;
            }
            return currentExecutableDirectory = new Uri(Path.GetDirectoryName(pathAttempt ?? ".") ?? Directory.GetCurrentDirectory()).LocalPath;
        }
    }

    internal class InstantLaunchData
    {
        public string SaveName { get; private set; }
        public string[] PlayerNames { get; private set; }

        public InstantLaunchData(string saveName, string[] playerNames)
        {
            SaveName = saveName;
            PlayerNames = playerNames;
        }
    }
}
