using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace Nitrox.Launcher.ViewModels;

internal partial class ServersViewModel : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore;
    private readonly DialogService dialogService;
    private readonly ServerService serverService;
    private readonly ManageServerViewModel manageServerViewModel;
    [ObservableProperty]
    private AvaloniaList<ServerEntry>? servers;

    public ServersViewModel(IKeyValueStore keyValueStore, DialogService dialogService, ServerService serverService, ManageServerViewModel manageServerViewModel)
    {
        this.keyValueStore = keyValueStore;
        this.dialogService = dialogService;
        this.serverService = serverService;
        this.manageServerViewModel = manageServerViewModel;

        this.RegisterMessageListener<ServerStatusMessage, ServersViewModel>((message, model) =>
        {
            ServerEntry entry = model.Servers?.FirstOrDefault(s => s.Process?.Id == message.ProcessId);
            if (entry == null)
            {
                return;
            }
            entry.Players = message.PlayerCount;
            entry.IsOnline = message.IsOnline;
        });

        serverService.PropertyChanged += ServerServiceOnPropertyChanged;
    }

    private void ServerServiceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(serverService.Servers))
        {
            Servers = [..serverService.Servers];
        }
    }

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        Servers = [..await serverService.GetServersAsync()];
        await serverService.DetectAndAttachRunningServersAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CreateServerAsync()
    {
        CreateServerViewModel? result = await dialogService.ShowAsync<CreateServerViewModel>();
        if (!result)
        {
            return;
        }

        try
        {
            ServerEntry serverEntry = await Task.Run(() => ServerEntry.FromDirectory(Path.Join(keyValueStore.GetSavesFolderDir(), result!.Name)));
            if (serverEntry == null)
            {
                throw new Exception("Failed to create save file");
            }
            // Don't add to servers list manually here, it will be added by file system watcher. Otherwise: possible duplicate entries by race-condition.
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error($"Server create failed: {ex.Message}");
            Log.Error(ex);
        }
    }

    [RelayCommand]
    public async Task<bool> StartServerAsync(ServerEntry server) => await serverService.StartServerAsync(server);

    [RelayCommand]
    public async Task ManageServer(ServerEntry server)
    {
        if (server.IsOnline && server.IsEmbedded)
        {
            ChangeView(new EmbeddedServerViewModel(server));
            return;
        }
        if (server.Version != NitroxEnvironment.Version && !await serverService.ConfirmServerVersionAsync(server))
        {
            return;
        }

        manageServerViewModel.LoadFrom(server);
        ChangeView(manageServerViewModel);
    }
}
