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
    private CancellationTokenSource serverRefreshCts;

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
            serverRefreshCts = new();
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
            GetSavesOnDisk();
            InitializeWatcher();

            // Deactivation
            Disposable
                .Create(this, vm =>
                {
                    WeakReferenceMessenger.Default.UnregisterAll(vm);
                    vm.watcher?.Dispose();
                    vm.serverRefreshCts.Cancel();
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

    public void GetSavesOnDisk()
    {
        try
        {
            Directory.CreateDirectory(keyValueStore.GetSavesFolderDir());

            List<ServerEntry> serversOnDisk = [];
            foreach (string saveDir in Directory.EnumerateDirectories(keyValueStore.GetSavesFolderDir()))
            {
                try
                {
                    ServerEntry entryFromDir = ServerEntry.FromDirectory(saveDir);
                    if (entryFromDir != null)
                    {
                        serversOnDisk.Add(entryFromDir);
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
                // Remove any servers from the Servers list that are not found in the saves folder
                for (int i = Servers.Count - 1; i >= 0; i--)
                {
                    if (serversOnDisk.All(s => s.Name != Servers[i].Name))
                    {
                        Servers.RemoveAt(i);
                    }
                }

                // Add any new servers found on the disk to the Servers list
                foreach (ServerEntry server in serversOnDisk)
                {
                    if (Servers.All(s => s.Name != server.Name) && !string.IsNullOrWhiteSpace(server.Name))
                    {
                        Servers.Add(server);
                    }
                }

                Servers = [..Servers.OrderByDescending(entry => entry.LastAccessedTime)];
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting saves");
            dialogService.ShowErrorAsync(ex, "Error while getting saves");
        }
    }

    private void InitializeWatcher()
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

        Task.Run(async () =>
        {
            watcher.EnableRaisingEvents = true; // Slowish (~2ms) - Moved into Task.Run.

            while (!serverRefreshCts.IsCancellationRequested)
            {
                while (shouldRefreshServersList)
                {
                    try
                    {
                        GetSavesOnDisk();
                        shouldRefreshServersList = false;
                    }
                    catch (IOException)
                    {
                        await Task.Delay(500);
                    }
                }
                await Task.Delay(1000);
            }
        }).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                LauncherNotifier.Error(t.Exception.Message);
            }
        });
    }

    private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
    {
        shouldRefreshServersList = true;
    }
}
