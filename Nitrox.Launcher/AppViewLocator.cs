using System;
using System.Collections.Concurrent;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher;

internal sealed class AppViewLocator : ViewLocatorBase, ReactiveUI.IViewLocator
{
    private static readonly ConcurrentDictionary<Type, RoutableViewModelBase> viewModelCache = new();
    private static MainWindow mainWindow;
    public static Lazy<AppViewLocator> Instance { get; } = new(new AppViewLocator());

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

    public override ViewDefinition Locate(object viewModel)
    {
        static Type GetViewType(object viewModel) => viewModel switch
        {
            MainWindowViewModel => typeof(MainWindow),
            LaunchGameViewModel => typeof(LaunchGameView),
            ServersViewModel => typeof(ServersView),
            ManageServerViewModel => typeof(ManageServerView),
            CreateServerViewModel => typeof(CreateServerModal),
            LibraryViewModel => typeof(LibraryView),
            CommunityViewModel => typeof(CommunityView),
            BlogViewModel => typeof(BlogView),
            UpdatesViewModel => typeof(UpdatesView),
            OptionsViewModel => typeof(OptionsView),
            ConfirmationBoxViewModel => typeof(ConfirmationBoxModal),
            ErrorViewModel => typeof(ErrorModal),
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel), viewModel, null)
        };

        // If the view type is the same as last time, return the same instance.
        Type newView = GetViewType(viewModel);
        return new ViewDefinition(newView, () => Activator.CreateInstance(newView));
    }

    public static TViewModel GetSharedViewModel<TViewModel>() where TViewModel : RoutableViewModelBase
    {
        Type key = typeof(TViewModel);
        if (viewModelCache.TryGetValue(key, out RoutableViewModelBase vm))
        {
            return (TViewModel)vm;
        }

        TViewModel viewModel = (TViewModel)key.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new[] { typeof(IScreen) })!.Invoke(new[] { MainWindow.ViewModel });
        viewModelCache.TryAdd(typeof(TViewModel), viewModel);
        return viewModel;
    }

    public IViewFor ResolveView<T>(T viewModel, string contract = null) => (IViewFor)Locate(viewModel).Create();
}
