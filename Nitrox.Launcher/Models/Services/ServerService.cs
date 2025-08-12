using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Keeps track of server instances.
/// </summary>
public class ServerService : IMessageReceiver, INotifyPropertyChanged
{
    private readonly IDialogService dialogService;
    private readonly IKeyValueStore keyValueStore;
    private readonly IRoutingScreen screen;
    private List<ServerEntry> servers = [];
    private readonly Lock serversLock = new();
    private bool shouldRefreshServersList;
    private FileSystemWatcher watcher;
    private readonly CancellationTokenSource serverRefreshCts = new();
    private readonly HashSet<string> loggedErrorDirectories = [];
    private volatile bool hasUpdatedAtLeastOnce;

    public ServerService(IDialogService dialogService, IKeyValueStore keyValueStore, IRoutingScreen screen)
    {
        this.dialogService = dialogService;
        this.keyValueStore = keyValueStore;
        this.screen = screen;

        _ = LoadServersAsync().ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));

        this.RegisterMessageListener<SaveDeletedMessage, ServerService>(static (message, receiver) =>
        {
            lock (receiver.serversLock)
            {
                bool changes = false;
                for (int i = receiver.servers.Count - 1; i >= 0; i--)
                {
                    if (receiver.servers[i].Name == message.SaveName)
                    {
                        receiver.servers.RemoveAt(i);
                        changes = true;
                    }
                }
                if (changes)
                {
                    receiver.SetField(ref receiver.servers, receiver.servers);
                }
            }
        });
    }

    private async Task LoadServersAsync()
    {
        await GetSavesOnDiskAsync();
        _ = WatchServersAsync(serverRefreshCts.Token).ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));
    }

    public async Task<bool> StartServerAsync(ServerEntry server)
    {
        // TODO: Exclude upgradeable versions + add separate prompt to upgrade first?
        if (server.Version != NitroxEnvironment.Version && !await ConfirmServerVersionAsync(server))
        {
            return false;
        }
        if (await GameInspect.IsOutdatedGameAndNotify(NitroxUser.GamePath, dialogService))
        {
            return false;
        }

        try
        {
            server.Version = NitroxEnvironment.Version;
            server.Start(keyValueStore.GetSavesFolderDir());
            if (server.IsEmbedded)
            {
                await screen.ShowAsync(new EmbeddedServerViewModel(server));
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while starting server \"{server.Name}\"");
            await Dispatcher.UIThread.InvokeAsync(async () => await dialogService.ShowErrorAsync(ex, $"Error while starting server \"{server.Name}\""));
            return false;
        }
    }

    public async Task<bool> ConfirmServerVersionAsync(ServerEntry server) =>
        await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = $"The version of '{server.Name}' is v{(server.Version != null ? server.Version.ToString() : "X.X.X.X")}. It is highly recommended to NOT use this save file with Nitrox v{NitroxEnvironment.Version}. Would you still like to continue?";
            model.ButtonOptions = ButtonOptions.YesNo;
        });

    private async Task GetSavesOnDiskAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(keyValueStore.GetSavesFolderDir());

            Dictionary<string, (ServerEntry Data, bool HasFiles)> serversOnDisk = Servers.ToDictionary(entry => entry.Name, entry => (entry, false));
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
                hasUpdatedAtLeastOnce = true;
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

    public void Dispose()
    {
        serverRefreshCts.Cancel();
        serverRefreshCts.Dispose();
        WeakReferenceMessenger.Default.UnregisterAll(this);
        watcher?.Dispose();
    }

    public ServerEntry[] Servers
    {
        get
        {
            lock (serversLock)
            {
                return [..servers];
            }
        }
        private set
        {
            lock (serversLock)
            {
                SetField(ref servers, [..value]);
            }
        }
    }

    /// <summary>
    /// Gets the servers or waits for servers to be loaded from file system.
    /// </summary>
    public async Task<ServerEntry[]> GetServersAsync()
    {
        while (true)
        {
            lock (serversLock)
            {
                if (hasUpdatedAtLeastOnce)
                {
                    return Servers;
                }
            }
            await Task.Delay(100);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public async Task<ServerEntry> GetOrCreateServerAsync(string saveName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveName);
        string serverPath = Path.Combine(keyValueStore.GetSavesFolderDir(), saveName);
        return (await GetServersAsync()).FirstOrDefault(s => s.Name == saveName) ?? ServerEntry.FromDirectory(serverPath) ?? ServerEntry.CreateNew(serverPath, NitroxGameMode.SURVIVAL);
    }
}
