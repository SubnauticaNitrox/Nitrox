using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher;

public class App : Application
{
    internal static Func<Window>? StartupWindowFactory;
    internal static InstantLaunchData? InstantLaunch;
    private static bool isCrashReport;

    /// <summary>
    ///     If true, allows duplicate instances of the app to be active.
    /// </summary>
    private static bool allowInstances;

    private static X11RenderingMode? preferredRenderingMode;

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
                case null when IsDesignMode:
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
        Log.Info($@"Starting Nitrox Launcher V{NitroxEnvironment.Version}+{NitroxEnvironment.GitHash} with args ""{string.Join(" ", NitroxEnvironment.CommandLineArgs)}"" built on {NitroxEnvironment.BuildDate:F}");

        // Handle command line arguments.
        ConsoleApp.ConsoleAppBuilder cliParser = ConsoleApp.Create();
        cliParser.Add("", (bool crashReport, X11RenderingMode? rendering = null, bool allowInstances = false) =>
        {
            isCrashReport = crashReport;
            preferredRenderingMode = rendering;
            App.allowInstances = allowInstances;

            if (isCrashReport && CrashReporter.GetLastReport() is {} crashLog)
            {
                StartupWindowFactory = () => new CrashWindow { DataContext = new CrashWindowViewModel { Title = $"Nitrox {NitroxEnvironment.Version} - Crash Report", Message = crashLog } };
            }
        });
        cliParser.Add("instantlaunch", ([SaveName] string save, [MinLength(1)] params string[] players) =>
        {
            InstantLaunch = new InstantLaunchData(save, players);
        });
        cliParser.Run(NitroxEnvironment.CommandLineArgs);

        // Fallback to normal startup.
        if (StartupWindowFactory == null)
        {
            if (!allowInstances)
            {
                CheckForRunningInstance();
            }
            ServiceProvider services = new ServiceCollection().AddAppServices().BuildServiceProvider();
            StartupWindowFactory = services.GetRequiredService<Func<Window>>();
        }

        AppBuilder builder = AppBuilder.Configure<App>()
                                       .UsePlatformDetect()
                                       .LogToTrace()
                                       .With(new SkiaOptions { UseOpacitySaveLayer = true });
        builder = WithRenderingMode(builder, preferredRenderingMode);
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
        Debug.Assert(StartupWindowFactory != null, $"{nameof(StartupWindowFactory)} != null");

        FixAvaloniaPlugins();
        ApplyAppDefaults();

        Dispatcher.UIThread.UnhandledException += (_, eventArgs) => HandleUnhandledException(eventArgs.Exception);
        AppWindow = StartupWindowFactory();
        base.OnFrameworkInitializationCompleted();
    }

    internal static void HandleUnhandledException(Exception ex)
    {
        if (isCrashReport)
        {
            Log.Error(ex, "Error while trying to show crash report");
            return;
        }

        CrashReporter.ReportAndExit(ex);
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

    private void ApplyAppDefaults()
    {
        RequestedThemeVariant = ThemeVariant.Dark;

        // April Fools: Switch to Comic Sans on April 1st (only works on OSes with Comic Sans installed).
        if (DateTime.Now is { Month: 4, Day: 1 })
        {
            Style? windowStyle = new(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.FontFamilyProperty, FontFamily.Parse("Comic Sans MS"))
                }
            };
            Styles.Add(windowStyle);
        }
    }

    internal record InstantLaunchData(string SaveName, string[] PlayerNames);
}
