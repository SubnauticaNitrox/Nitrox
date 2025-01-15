using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
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
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private string maximizeButtonIcon = "/Assets/Images/material-design-icons/max.png";

    [ObservableProperty]
    private bool updateAvailableOrUnofficial;

    public AvaloniaList<NotificationItem> Notifications { get; init; } = [];

    [ObservableProperty]
    private IRoutingScreen routingScreen;

    [ObservableProperty]
    private object activeViewModel;

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
        OptionsViewModel optionsViewModel,
        IDialogService dialogService
    )
    {
        this.launchGameViewModel = launchGameViewModel;
        this.serversViewModel = serversViewModel;
        this.communityViewModel = communityViewModel;
        this.blogViewModel = blogViewModel;
        this.updatesViewModel = updatesViewModel;
        this.optionsViewModel = optionsViewModel;
        this.routingScreen = routingScreen;
        this.dialogService = dialogService;

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(model => model.RoutingScreen.ActiveViewModel)
                .Subscribe(viewModel => ActiveViewModel = viewModel)
                .DisposeWith(disposables);

            RegisterMessageListener<NotificationAddMessage, MainWindowViewModel>(static async (message, vm) =>
            {
                vm.Notifications.Add(message.Item);
                await Task.Delay(7000);
                WeakReferenceMessenger.Default.Send(new NotificationCloseMessage(message.Item));
            });
            RegisterMessageListener<NotificationCloseMessage, MainWindowViewModel>(static async (message, vm) =>
            {
                message.Item.Dismissed = true;
                await Task.Delay(1000); // Wait for animations
                if (!Design.IsDesignMode) // Prevent design preview crashes
                {
                    vm.Notifications.Remove(message.Item);
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
                UpdateAvailableOrUnofficial = await updatesViewModel.IsNitroxUpdateAvailableAsync();
            });

            Disposable.Create(this, vm =>
            {
                WeakReferenceMessenger.Default.UnregisterAll(vm);
            }).DisposeWith(disposables);
        });

        ActiveViewModel = this.launchGameViewModel;
        _ = RoutingScreen.ShowAsync(launchGameViewModel).ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenLaunchGameViewAsync()
    {
        await RoutingScreen.ShowAsync(launchGameViewModel);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenServersViewAsync()
    {
        await RoutingScreen.ShowAsync(serversViewModel);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenCommunityViewAsync()
    {
        await RoutingScreen.ShowAsync(communityViewModel);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenBlogViewAsync()
    {
        await RoutingScreen.ShowAsync(blogViewModel);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenUpdatesViewAsync()
    {
        await RoutingScreen.ShowAsync(updatesViewModel);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task OpenOptionsViewAsync()
    {
        await RoutingScreen.ShowAsync(optionsViewModel);
    }

    [RelayCommand]
    public async Task ClosingAsync(WindowClosingEventArgs args)
    {
        ServerEntry[] onlineServers = serversViewModel?.Servers.Where(s => s.IsOnline).ToArray() ?? [];
        if (onlineServers.Length > 0)
        {
            DialogBoxViewModel result = await ShowDialogAsync(dialogService, args, "Do you want to stop all online servers?");
            if (result)
            {
                // Closing servers can take a while: hide the main window.
                MainWindow.Hide();
                try
                {
                    await Task.WhenAll(onlineServers.Select(s => s.StopAsync()));
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        // As closing handler isn't async, cancellation might have happened anyway. So check manually if we should close the window after all the tasks are done.
        if (args.Cancel == false && MainWindow.IsClosingByUser())
        {
            MainWindow.CloseByCode();
        }

        static async Task<DialogBoxViewModel> ShowDialogAsync(IDialogService dialogService, WindowClosingEventArgs args, string title)
        {
            // Showing dialogs doesn't work if closing isn't set as 'cancelled'.
            bool prevCancelFlag = args.Cancel;
            args.Cancel = true;
            try
            {
                return await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                {
                    model.Title = title;
                    model.ButtonOptions = ButtonOptions.YesNo;
                });
            }
            finally
            {
                args.Cancel = prevCancelFlag;
            }
        }
    }
}
