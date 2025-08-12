using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;

namespace Nitrox.Launcher;

public sealed class AppViewLocator : ViewLocatorBase
{
    private static IServiceProvider serviceProvider;

    public AppViewLocator(IServiceProvider serviceProvider)
    {
        AppViewLocator.serviceProvider = serviceProvider;
    }

    private static MainWindow mainWindow;
    public static MainWindow MainWindow
    {
        get
        {
            if (mainWindow != null)
            {
                return mainWindow;
            }

            if (Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
            {
                return mainWindow = (MainWindow)desktop.MainWindow;
            }
            throw new NotSupportedException("This Avalonia application is only supported on desktop environments.");
        }
    }

    public static IRoutingScreen HostScreen => serviceProvider.GetRequiredService<IRoutingScreen>();

    public override ViewDefinition Locate(object viewModel)
    {
        // Only dialogs need to be mapped here. Other views are handled in MainWindow.axaml.
        static Type GetViewType(object viewModel) => viewModel switch
        {
            CreateServerViewModel => typeof(CreateServerModal),
            DialogBoxViewModel => typeof(DialogBoxModal),
            ObjectPropertyEditorViewModel => typeof(ObjectPropertyEditorModal),
            BackupRestoreViewModel => typeof(BackupRestoreModal),
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), viewModel, null)
        };

        Type newView = GetViewType(viewModel);
        return new ViewDefinition(newView, () => serviceProvider.GetRequiredService(newView));
    }
}
