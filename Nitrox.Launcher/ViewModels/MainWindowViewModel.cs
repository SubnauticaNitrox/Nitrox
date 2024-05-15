using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly IDialogService dialogService;
    public RoutingState Router { get; } = new();
    public ICommand DefaultViewCommand { get; }
    [ObservableProperty]
    private string maximizeButtonIcon = "/Assets/Images/material-design-icons/max-w-10.png";

    public AvaloniaList<NotificationItem> Notifications { get; init; }

    public MainWindowViewModel(IDialogService dialogService, IList<NotificationItem> notifications = null)
    {
        this.dialogService = dialogService;

        DefaultViewCommand = OpenLaunchGameViewCommand;
        Notifications = notifications == null ? [] : [..notifications];

        WeakReferenceMessenger.Default.Register<NotificationAddMessage>(this, (_, message) =>
        {
            Notifications.Add(message.Item);
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                Notifications.Remove(message.Item);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Log.Error(t.Exception, "Failed to remove notification after a time delay");
                }
            });
        });
        WeakReferenceMessenger.Default.Register<NotificationCloseMessage>(this, (_, message) => Notifications.Remove(message.Item));
    }

    [RelayCommand]
    public void OpenLaunchGameView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<LaunchGameViewModel>());
    }

    [RelayCommand]
    public void OpenServersView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<ServersViewModel>());
    }

    [RelayCommand]
    public void OpenCommunityView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<CommunityViewModel>());
    }

    [RelayCommand]
    public void OpenBlogView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<BlogViewModel>());
    }

    [RelayCommand]
    public void OpenUpdatesView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<UpdatesViewModel>());
    }

    [RelayCommand]
    public void OpenOptionsView()
    {
        Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<OptionsViewModel>());
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

    public async Task<T> ShowDialogAsync<T, TExtra>(Action<T, TExtra> setup = null, TExtra extraParameter = default) where T : ModalViewModelBase
    {
        ArgumentNullException.ThrowIfNull(dialogService);

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
