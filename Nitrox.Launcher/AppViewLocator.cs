using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher;

public sealed class AppViewLocator : ViewLocatorBase, ReactiveUI.IViewLocator
{
    private static IServiceProvider serviceProvider;

    public AppViewLocator(IServiceProvider serviceProvider)
    {
        if (serviceProvider is IServiceScope)
        {
            AppViewLocator.serviceProvider = serviceProvider;
        }
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

    public static IScreen HostScreen => serviceProvider.GetRequiredService<IScreen>();

    public override ViewDefinition Locate(object viewModel)
    {
        static Type GetViewType(object viewModel) => viewModel switch
        {
            MainWindowViewModel => typeof(MainWindow),
            LaunchGameViewModel => typeof(LaunchGameView),
            ServersViewModel => typeof(ServersView),
            ManageServerViewModel => typeof(ManageServerView),
            CreateServerViewModel => typeof(CreateServerModal),
            EmbeddedServerViewModel => typeof(EmbeddedServerView),
            LibraryViewModel => typeof(LibraryView),
            CommunityViewModel => typeof(CommunityView),
            BlogViewModel => typeof(BlogView),
            UpdatesViewModel => typeof(UpdatesView),
            OptionsViewModel => typeof(OptionsView),
            DialogBoxViewModel => typeof(DialogBoxModal),
            ObjectPropertyEditorViewModel => typeof(ObjectPropertyEditorModal),
            BackupRestoreViewModel => typeof(BackupRestoreModal),
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), viewModel, null)
        };

        Type newView = GetViewType(viewModel);
        return new ViewDefinition(newView, () => serviceProvider.GetRequiredService(newView));
    }

    public IViewFor ResolveView<T>(T viewModel, string contract = null) => (IViewFor)Locate(viewModel).Create();
}
