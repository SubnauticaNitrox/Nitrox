using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
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

public class App : Application
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

    public Window AppWindow
    {
        set
        {
            switch (ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.MainWindow = value;
                    break;
                case ISingleViewApplicationLifetime singleViewPlatform:
                    singleViewPlatform.MainView = value;
                    break;
                case null when Design.IsDesignMode:
                    Log.Info("Running in design previewer!");
                    break;
                default:
                    throw new NotSupportedException($"Current platform '{ApplicationLifetime?.GetType().Name}' is not supported by {nameof(Nitrox)}.{nameof(Launcher)}");
            }
        }
    }

    public static AppBuilder Create()
    {
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

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        FixAvaloniaPlugins();
        ApplyAppDefaults();

        Dispatcher.UIThread.UnhandledException += (_, eventArgs) => HandleUnhandledException(eventArgs.Exception);
        AppWindow = StartupWindowFactory();
        base.OnFrameworkInitializationCompleted();
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
                ProcessUtils.StartSelf("--crash-report");
            }
            else
            {
                Log.Error(ex, "Unable to get executable file path for writing crash report.");
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        Environment.Exit(1);
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

    /// <summary>
    ///     Disables Avalonia plugins which are replaced by MVVM Toolkit.
    /// </summary>
    private void FixAvaloniaPlugins()
    {
        for (int i = BindingPlugins.DataValidators.Count - 1; i >= 0; i--)
        {
            if (BindingPlugins.DataValidators[i] is DataAnnotationsValidationPlugin)
            {
                BindingPlugins.DataValidators.RemoveAt(i);
            }
        }
    }

    private void ApplyAppDefaults() => RequestedThemeVariant = ThemeVariant.Dark;

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
