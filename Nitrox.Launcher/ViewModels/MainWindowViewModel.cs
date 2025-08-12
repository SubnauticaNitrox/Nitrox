using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;

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
    private readonly ServerService serverService;

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
        IDialogService dialogService,
        ServerService serverService
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
        this.serverService = serverService;

        this.RegisterMessageListener<ViewShownMessage, MainWindowViewModel>(static (message, vm) => vm.ActiveViewModel = message.ViewModel);
        this.RegisterMessageListener<NotificationAddMessage, MainWindowViewModel>(static async (message, vm) =>
        {
            vm.Notifications.Add(message.Item);
            await Task.Delay(7000);
            WeakReferenceMessenger.Default.Send(new NotificationCloseMessage(message.Item));
        });
        this.RegisterMessageListener<NotificationCloseMessage, MainWindowViewModel>(static async (message, vm) =>
        {
            message.Item.Dismissed = true;
            await Task.Delay(1000); // Wait for animations
            if (!Design.IsDesignMode) // Prevent design preview crashes
            {
                vm.Notifications.Remove(message.Item);
            }
        });

        if (!Design.IsDesignMode)
        {
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
        }

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
        ServerEntry[] embeddedServers = serverService.Servers.Where(s => s.IsOnline && s.IsEmbedded).ToArray();
        if (embeddedServers.Length > 0)
        {
            DialogBoxViewModel result = await ShowDialogAsync(dialogService, args, $"{embeddedServers.Length} embedded server(s) will stop, continue?");
            if (!result)
            {
                args.Cancel = true;
                return;
            }

            await HideWindowAndStopServersAsync(MainWindow, embeddedServers);
        }

        // As closing handler isn't async, cancellation might have happened anyway. So check manually if we should close the window after all the tasks are done.
        if (args.Cancel == false && MainWindow.IsClosingByUser(args))
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

        static async Task HideWindowAndStopServersAsync(Window mainWindow, IEnumerable<ServerEntry> servers)
        {
            // Closing servers can take a while: hide the main window.
            mainWindow.Hide();
            try
            {
                await Task.WhenAll(servers.Select(s => s.StopAsync()));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
