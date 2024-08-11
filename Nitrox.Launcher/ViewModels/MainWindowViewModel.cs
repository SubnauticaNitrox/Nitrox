using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly BlogViewModel blogViewModel;
    private readonly CommunityViewModel communityViewModel;
    private readonly LaunchGameViewModel launchGameViewModel;
    private readonly OptionsViewModel optionsViewModel;
    private readonly IScreen screen;
    private readonly ServersViewModel serversViewModel;
    private readonly UpdatesViewModel updatesViewModel;

    [ObservableProperty]
    private string maximizeButtonIcon = "/Assets/Images/material-design-icons/max-w-10.png";

    [ObservableProperty]
    private bool updateAvailableOrUnofficial;

    public ICommand DefaultViewCommand { get; }

    public AvaloniaList<NotificationItem> Notifications { get; init; }

    public RoutingState Router => screen.Router;

    public MainWindowViewModel(
        IScreen screen,
        ServersViewModel serversViewModel,
        LaunchGameViewModel launchGameViewModel,
        CommunityViewModel communityViewModel,
        BlogViewModel blogViewModel,
        UpdatesViewModel updatesViewModel,
        OptionsViewModel optionsViewModel,
        IList<NotificationItem> notifications = null
    )
    {
        this.screen = screen;
        this.launchGameViewModel = launchGameViewModel;
        this.serversViewModel = serversViewModel;
        this.communityViewModel = communityViewModel;
        this.blogViewModel = blogViewModel;
        this.updatesViewModel = updatesViewModel;
        this.optionsViewModel = optionsViewModel;

        DefaultViewCommand = OpenLaunchGameViewCommand;
        Notifications = notifications == null ? [] : [.. notifications];

        WeakReferenceMessenger.Default.Register<NotificationAddMessage>(this, (_, message) =>
        {
            Notifications.Add(message.Item);
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                WeakReferenceMessenger.Default.Send(new NotificationCloseMessage(message.Item));
            });
        });
        WeakReferenceMessenger.Default.Register<NotificationCloseMessage>(this, async (_, message) =>
        {
            message.Item.Dismissed = true;
            await Task.Delay(1000); // Wait for animations
            if (!Design.IsDesignMode) // Prevent design preview crashes
            {
                Notifications.Remove(message.Item);
            }
        });

        if (!NitroxEnvironment.IsReleaseMode)
        {
            LauncherNotifier.Info("You're now using Nitrox DEV build");
        }

        Task.Run(async () =>
        {
            if (!await NetHelper.HasInternetConnectivityAsync())
            {
                Log.Warn("Launcher may not be connected to internet");
                LauncherNotifier.Warning("Launcher may not be connected to internet");
            }
            UpdateAvailableOrUnofficial = await UpdatesViewModel.IsNitroxUpdateAvailableAsync();
        });
    }

    [RelayCommand]
    public void OpenLaunchGameView()
    {
        screen.Show(launchGameViewModel);
    }

    [RelayCommand]
    public void OpenServersView()
    {
        screen.Show(serversViewModel);
    }

    [RelayCommand]
    public void OpenCommunityView()
    {
        screen.Show(communityViewModel);
    }

    [RelayCommand]
    public void OpenBlogView()
    {
        screen.Show(blogViewModel);
    }

    [RelayCommand]
    public void OpenUpdatesView()
    {
        screen.Show(updatesViewModel);
    }

    [RelayCommand]
    public void OpenOptionsView()
    {
        screen.Show(optionsViewModel);
    }

    [RelayCommand]
    public void Minimize()
    {
        MainWindow.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    public void Close()
    {
        MainWindow.Close();
    }

    [RelayCommand]
    public void Maximize()
    {
        if (MainWindow.WindowState == WindowState.Normal)
        {
            MainWindow.WindowState = WindowState.Maximized;
            MaximizeButtonIcon = "/Assets/Images/material-design-icons/restore-w-10.png";
        }
        else
        {
            MainWindow.WindowState = WindowState.Normal;
            MaximizeButtonIcon = "/Assets/Images/material-design-icons/max-w-10.png";
        }
    }

    [RelayCommand]
    public void Drag(PointerPressedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Source is Visual element && element.GetWindow() is { } window)
        {
            window.BeginMoveDrag(args);
        }
    }
}
