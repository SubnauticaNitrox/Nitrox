using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
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

public partial class ServersViewModel : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore;
    private readonly IDialogService dialogService;
    private readonly ServerService serverService;
    private readonly ManageServerViewModel manageServerViewModel;
    private readonly Lock serversLock = new();

    [ObservableProperty]
    private AvaloniaList<ServerEntry> servers = [];

    private bool shouldRefreshServersList;

    private FileSystemWatcher watcher;
    private CancellationTokenSource serverRefreshCts;

    private readonly HashSet<string> loggedErrorDirectories = [];

    public ServersViewModel()
    {
    }

    public ServersViewModel(IKeyValueStore keyValueStore, IDialogService dialogService, ServerService serverService, ManageServerViewModel manageServerViewModel)
    {
        this.keyValueStore = keyValueStore;
        this.dialogService = dialogService;
        this.serverService = serverService;
        this.manageServerViewModel = manageServerViewModel;
    }

    internal override async Task ViewContentLoadAsync()
    {
        try
        {
            WeakReferenceMessenger.Default.Register<SaveDeletedMessage>(this, (sender, message) =>
            {
                lock (serversLock)
                {
                    for (int i = Servers.Count - 1; i >= 0; i--)
                    {
                        if (Servers[i].Name == message.SaveName)
                        {
                            Servers.RemoveAt(i);
                        }
                    }
                }
            });
        }
        catch (InvalidOperationException)
        {
            // ignored - already subscribed exception.
        }
        // Load server list
        serverRefreshCts = new();
        await GetSavesOnDiskAsync();
        _ = WatchServersAsync(serverRefreshCts.Token).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                LauncherNotifier.Error(t.Exception.Message);
            }
        });
    }

    internal override Task ViewContentUnloadAsync()
    {
        serverRefreshCts.Cancel();
        serverRefreshCts.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
        watcher?.Dispose();
        return Task.CompletedTask;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task CreateServerAsync()
    {
        CreateServerViewModel result = await dialogService.ShowAsync<CreateServerViewModel>();
        if (!result)
        {
            return;
        }

        try
        {
            ServerEntry serverEntry = await Task.Run(() => ServerEntry.FromDirectory(Path.Join(keyValueStore.GetSavesFolderDir(), result.Name)));
            if (serverEntry == null)
            {
                throw new Exception("Failed to create save file");
            }
            lock (serversLock)
            {
                Servers.Insert(0, serverEntry);
            }
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error($"Server create failed: {ex.Message}");
            Log.Error(ex);
        }
    }

    [RelayCommand]
    public async Task<bool> StartServerAsync(ServerEntry server)
    {
        return await serverService.StartServerAsync(server);
    }

    [RelayCommand]
    public async Task ManageServer(ServerEntry server)
    {
        if (server.IsOnline && server.IsEmbedded)
        {
            await HostScreen.ShowAsync(new EmbeddedServerViewModel(server));
            return;
        }
        if (server.Version != NitroxEnvironment.Version && !await serverService.ConfirmServerVersionAsync(server)) // TODO: Exclude upgradeable versions + add separate prompt to upgrade first?
        {
            return;
        }

        manageServerViewModel.LoadFrom(server);
        await HostScreen.ShowAsync(manageServerViewModel);
    }

    private async Task GetSavesOnDiskAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(keyValueStore.GetSavesFolderDir());

            Dictionary<string, (ServerEntry Data, bool HasFiles)> serversOnDisk;
            lock (serversLock)
            {
                serversOnDisk = Servers.ToDictionary(entry => entry.Name, entry => (entry, false));
            }
            foreach (string saveDir in Directory.EnumerateDirectories(keyValueStore.GetSavesFolderDir()))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (serversOnDisk.TryGetValue(Path.GetFileName(saveDir), out (ServerEntry Data, bool _) server))
                    {
                        // This server has files, so don't filter it away from server list.
                        serversOnDisk[Path.GetFileName(saveDir)] = (server.Data, true);
                        continue;
                    }
                    ServerEntry entryFromDir = await Task.Run(() => ServerEntry.FromDirectory(saveDir), cancellationToken);
                    if (entryFromDir != null)
                    {
                        serversOnDisk.Add(entryFromDir.Name, (entryFromDir, true));
                    }
                    loggedErrorDirectories.Remove(saveDir);
                }
                catch (Exception ex)
                {
                    if (loggedErrorDirectories.Add(saveDir)) // Only log once per directory to prevent log spam
                    {
                        Log.Error(ex, $"Error while initializing save from directory \"{saveDir}\". Skipping...");
                    }
                }
            }

            lock (serversLock)
            {
                Servers = [..serversOnDisk.Values.Where(server => server.HasFiles).Select(server => server.Data).OrderByDescending(entry => entry.LastAccessedTime)];
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            Log.Error(ex, "Error while getting saves");
            await dialogService.ShowErrorAsync(ex, "Error while getting saves");
        }
    }

    private async Task WatchServersAsync(CancellationToken cancellationToken = default)
    {
        watcher = new FileSystemWatcher
        {
            Path = keyValueStore.GetSavesFolderDir(),
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size,
            Filter = "*.*",
            IncludeSubdirectories = true
        };
        watcher.Changed += OnDirectoryChanged;
        watcher.Created += OnDirectoryChanged;
        watcher.Deleted += OnDirectoryChanged;
        watcher.Renamed += OnDirectoryChanged;

        try
        {
            await Task.Run(async () =>
            {
                watcher.EnableRaisingEvents = true; // Slowish (~2ms) - Moved into Task.Run.

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    while (shouldRefreshServersList)
                    {
                        try
                        {
                            await GetSavesOnDiskAsync(cancellationToken);
                            shouldRefreshServersList = false;
                        }
                        catch (IOException)
                        {
                            await Task.Delay(100, cancellationToken);
                        }
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
    {
        shouldRefreshServersList = true;
    }
}
