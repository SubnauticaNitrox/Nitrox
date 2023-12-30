using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly IDialogService dialogService;
    public RoutingState Router { get; } = new();
    public List<INavigationItem> NavigationHeaderItems { get; }
    public List<INavigationItem> NavigationFooterItems { get; }
    public List<TitleBarItem> TitleBarItems { get; }
    public ICommand DefaultCommand { get; }

    public MainWindowViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
        TitleBarItem maximizeControl = new()
        {
            Icon = "/Assets/Images/material-design-icons/max-w-10.png"
        };
        maximizeControl.Command = ReactiveCommand.Create(() =>
        {
            if (MainWindow.WindowState == WindowState.Normal)
            {
                MainWindow.WindowState = WindowState.Maximized;
                maximizeControl.Icon = "/Assets/Images/material-design-icons/restore-w-10.png";
            }
            else
            {
                MainWindow.WindowState = WindowState.Normal;
                maximizeControl.Icon = "/Assets/Images/material-design-icons/max-w-10.png";
            }
        });
        TitleBarItems = new List<TitleBarItem>
        {
            new()
            {
                Command = ReactiveCommand.Create(() => MainWindow.WindowState = WindowState.Minimized),
                Icon = "/Assets/Images/material-design-icons/min-w-10.png"
            },
            maximizeControl,
            new()
            {
                Command = ReactiveCommand.Create(() => MainWindow.Close()),
                Icon = "/Assets/Images/material-design-icons/close-w-10.png",
                HoverBackgroundColor = new SolidColorBrush(Colors.Red)
            }
        };

        DefaultCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<LaunchGameViewModel>()));
        NavigationHeaderItems = new List<INavigationItem>
        {
            new NavigationHeader("PLAY"),
            new NavigationItem("Play game")
            {
                ToolTipText = "Play the game",
                Icon = "/Assets/Images/material-design-icons/play.png",
                ClickCommand = DefaultCommand
            },
            new NavigationItem("Servers")
            {
                ToolTipText = "Configure and start the server",
                Icon = "/Assets/Images/material-design-icons/server.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<ServersViewModel>()))
            },
            new NavigationItem("Library")
            {
                ToolTipText = "Configure your setup",
                Icon = "/Assets/Images/material-design-icons/library.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<PlayViewModel>()))
            },
            new NavigationHeader("EXPLORE"),
            new NavigationItem("Community")
            {
                ToolTipText = "Join the Nitrox community",
                Icon = "/Assets/Images/material-design-icons/community.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<PlayViewModel>()))
            },
            new NavigationItem("Blog")
            {
                ToolTipText = "Read the latest from the Dev Blog",
                Icon = "/Assets/Images/material-design-icons/blog.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<PlayViewModel>()))
            }
        };

        NavigationFooterItems = new List<INavigationItem>
        {
            new NavigationItem("Updates")
            {
                Icon = "/Assets/Images/material-design-icons/download.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<PlayViewModel>()))
            },
            new NavigationItem("Options")
            {
                Icon = "/Assets/Images/material-design-icons/options.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<PlayViewModel>()))
            }
        };
    }

    public async Task<T> ShowDialogAsync<T, TExtra>(Action<T, TExtra> setup = null, TExtra extraParameter = default) where T : ModalViewModelBase
    {
        T viewModel = dialogService.CreateViewModel<T>();
        setup?.Invoke(viewModel, extraParameter);
        bool? result = await dialogService.ShowDialogAsync<T>(this, viewModel);
        if (result == true)
        {
            return viewModel;
        }
        return default;
    }

    public Task<T> ShowDialogAsync<T>(Action<T> setup = null) where T : ModalViewModelBase => ShowDialogAsync<T,Action<T>>((model, act) => act?.Invoke(model), setup);
}
