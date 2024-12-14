using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
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
    private readonly ServersViewModel serversViewModel;
    private readonly UpdatesViewModel updatesViewModel;

    [ObservableProperty]
    private string maximizeButtonIcon = "/Assets/Images/material-design-icons/max.png";

    [ObservableProperty]
    private bool updateAvailableOrUnofficial;

    public AvaloniaList<NotificationItem> Notifications { get; init; } = [];

    [ObservableProperty]
    private IRoutingScreen routingScreen;

    [ObservableProperty]
    private RoutableViewModelBase activeViewModel;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(
        IRoutingScreen routingScreen,
        ServersViewModel serversViewModel,
        LaunchGameViewModel launchGameViewModel,
        CommunityViewModel communityViewModel,
        BlogViewModel blogViewModel,
        UpdatesViewModel updatesViewModel,
        OptionsViewModel optionsViewModel
    )
    {
        this.launchGameViewModel = launchGameViewModel;
        this.serversViewModel = serversViewModel;
        this.communityViewModel = communityViewModel;
        this.blogViewModel = blogViewModel;
        this.updatesViewModel = updatesViewModel;
        this.optionsViewModel = optionsViewModel;
        this.routingScreen = routingScreen;

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(model => model.RoutingScreen.ActiveViewModel)
                .Subscribe(viewModel => ActiveViewModel = viewModel)
                .DisposeWith(disposables);

            WeakReferenceMessenger.Default.Register<NotificationAddMessage>(this, (_, message) =>
            {
                Notifications.Add(message.Item);
                Task.Run(async () =>
                {
                    await Task.Delay(7000);
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
            
            Disposable.Create(this, vm =>
            {
                WeakReferenceMessenger.Default.UnregisterAll(vm);
            }).DisposeWith(disposables);
        });

        _ = RoutingScreen.ShowAsync(launchGameViewModel).ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));
    }

    [RelayCommand]
    public async Task OpenLaunchGameViewAsync()
    {
        await RoutingScreen.ShowAsync(launchGameViewModel);
    }

    [RelayCommand]
    public async Task OpenServersViewAsync()
    {
        await RoutingScreen.ShowAsync(serversViewModel);
    }

    [RelayCommand]
    public async Task OpenCommunityViewAsync()
    {
        await RoutingScreen.ShowAsync(communityViewModel);
    }

    [RelayCommand]
    public async Task OpenBlogViewAsync()
    {
        await RoutingScreen.ShowAsync(blogViewModel);
    }

    [RelayCommand]
    public async Task OpenUpdatesViewAsync()
    {
        await RoutingScreen.ShowAsync(updatesViewModel);
    }

    [RelayCommand]
    public async Task OpenOptionsViewAsync()
    {
        await RoutingScreen.ShowAsync(optionsViewModel);
    }
}
