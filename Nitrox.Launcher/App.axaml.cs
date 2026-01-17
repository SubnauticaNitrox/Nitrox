using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal class App : Application
{
    internal static Func<Window>? StartupWindowFactory;
    internal static InstantLaunchData? InstantLaunch;
    private static bool isCrashReport;
    public static App Instance;

    /// <summary>
    ///     If true, allows duplicate instances of the app to be active.
    /// </summary>
    internal static bool allowInstances;

    private static X11RenderingMode? preferredRenderingMode;

    public Window? AppWindow
    {
        get =>
            ApplicationLifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
                _ => throw new NotSupportedException($"Current platform '{ApplicationLifetime?.GetType().Name}' is not supported by {nameof(Nitrox)}.{nameof(Launcher)}")
            };
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
        StartupWindowFactory ??= () =>
        {
            WebApplicationBuilder hostBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
                ApplicationName = NitroxConstants.LAUNCHER_APP_NAME,
                Args = NitroxEnvironment.CommandLineArgs
            });
            hostBuilder.Logging
                       .ClearProviders()
                       .Services
                       .AddAppServices();
            hostBuilder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Any, 0, o => o.Protocols = HttpProtocols.Http2));
            WebApplication host = hostBuilder.Build();
            host.MapMagicOnionService();
            host.MapGrpcService<ServersManagement>();
            host.RunAsync().ContinueWithHandleError();
            return host.Services.GetRequiredService<Func<Window>>()();
        };

        return CreateAvaloniaBuilder();

        static AppBuilder CreateAvaloniaBuilder()
        {
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
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Instance = this;
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
