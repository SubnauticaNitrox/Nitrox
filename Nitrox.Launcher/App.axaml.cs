using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using NitroxModel.Logger;

namespace Nitrox.Launcher;

public class App : Application
{
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

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        FixAvaloniaPlugins();
        ApplyAppDefaults();

        Dispatcher.UIThread.UnhandledException += (_, eventArgs) => Program.HandleUnhandledException(eventArgs.Exception);
        AppWindow = Program.StartupWindowFactory();
        base.OnFrameworkInitializationCompleted();
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
}
