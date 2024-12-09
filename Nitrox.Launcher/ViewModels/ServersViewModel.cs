using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
using ReactiveUI;

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

    private readonly HashSet<string> loggedErrorDirectories = [];

    public ServersViewModel(IScreen screen, IKeyValueStore keyValueStore, IDialogService dialogService, ServerService serverService, ManageServerViewModel manageServerViewModel) : base(screen)
    {
        this.keyValueStore = keyValueStore;
        this.dialogService = dialogService;
        this.serverService = serverService;
        this.manageServerViewModel = manageServerViewModel;

        this.WhenActivated(disposables =>
        {
            // Activation
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
            // Load server list
            CancellationTokenSource serverRefreshCts = new();
            _ = LoadServersAndWatchAsync(serverRefreshCts.Token).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    LauncherNotifier.Error(t.Exception.Message);
                }
            });

            // Deactivation
            Disposable
                .Create(this, vm =>
                {
                    serverRefreshCts.Cancel();
                    serverRefreshCts.Dispose();
                    WeakReferenceMessenger.Default.UnregisterAll(vm);
                    vm.watcher?.Dispose();
                })
                .DisposeWith(disposables);
        });
    }

    [RelayCommand]
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
            HostScreen.Show(new EmbeddedServerViewModel(HostScreen, server));
            return;
        }
        if (server.Version != NitroxEnvironment.Version && !await serverService.ConfirmServerVersionAsync(server)) // TODO: Exclude upgradeable versions + add separate prompt to upgrade first?
        {
            return;
        }

        manageServerViewModel.LoadFrom(server);
        HostScreen.Show(manageServerViewModel);
    }

    private async Task GetSavesOnDiskAsync()
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
                try
                {
                    if (serversOnDisk.TryGetValue(Path.GetFileName(saveDir), out (ServerEntry Data, bool _) server))
                    {
                        // This server has files, so don't filter it away from server list.
                        serversOnDisk[Path.GetFileName(saveDir)] = (server.Data, true);
                        continue;
                    }
                    ServerEntry entryFromDir = await Task.Run(() => ServerEntry.FromDirectory(saveDir));
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
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting saves");
            await dialogService.ShowErrorAsync(ex, "Error while getting saves");
        }
    }

    private async Task LoadServersAndWatchAsync(CancellationToken cancellationToken = default)
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

        await Task.Run(async () =>
        {
            watcher.EnableRaisingEvents = true; // Slowish (~2ms) - Moved into Task.Run.

            await GetSavesOnDiskAsync(); // First time load
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (shouldRefreshServersList)
                {
                    try
                    {
                        await GetSavesOnDiskAsync();
                        shouldRefreshServersList = false;
                    }
                    catch (IOException)
                    {
                        await Task.Delay(500, cancellationToken);
                    }
                }
                await Task.Delay(1000, cancellationToken);
            }
        }, cancellationToken);
    }

    private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
    {
        shouldRefreshServersList = true;
    }
}
