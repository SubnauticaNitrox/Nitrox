using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NitroxModel.Logger;

namespace Nitrox.Launcher;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Disable Avalonia plugins replaced by MVVM Toolkit.
        for (int i = BindingPlugins.DataValidators.Count - 1; i >= 0; i--)
        {
            if (BindingPlugins.DataValidators[i] is DataAnnotationsValidationPlugin)
            {
                BindingPlugins.DataValidators.RemoveAt(i);
            }
        }

        ServiceProvider services = new ServiceCollection().AddAppServices()
                                                          .BuildServiceProvider();

        MainWindow mainWindow = services.GetRequiredService<MainWindow>();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = mainWindow;
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = mainWindow;
                break;
            case null when Design.IsDesignMode:
                Log.Info("Running in design previewer!");
                break;
            default:
                throw new NotSupportedException($"Current platform '{ApplicationLifetime?.GetType().Name}' is not supported by {nameof(Nitrox)}.{nameof(Launcher)}");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
