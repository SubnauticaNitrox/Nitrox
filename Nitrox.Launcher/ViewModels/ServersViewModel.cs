using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Collections;
using DynamicData.Binding;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Upgrade;
using NitroxServer.Serialization.World;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class ServersViewModel : RoutableViewModelBase
{
    public ICommand CreateServerCommand { get; }
    public ICommand ManageServerCommand { get; }
    public AvaloniaList<ServerEntry> Servers { get; } = new();

    public ServersViewModel(IScreen hostScreen) : base(hostScreen)
    {
        //IObservable<bool> canExecuteManageServerCommand = this.WhenAnyPropertyChanged().Select(x => !x.HasChanges());
        
        CreateServerCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CreateServerViewModel result = await ShowDialogAsync(MainViewModel.CreateServerDialog);
            if (result == null)
            {
                return;
            }

            AddServer(result.Name, result.SelectedGameMode);
        });
        ManageServerCommand = ReactiveCommand.Create((ServerEntry server) =>
        {
            ManageServerViewModel viewModel = Locator.GetSharedViewModel<ManageServerViewModel>();
            viewModel.LoadFrom(server);
            MainViewModel.Router.Navigate.Execute(viewModel);
        }/*, canExecuteManageServerCommand*/);
        
        // Load servers from the saves folder
        foreach (WorldManager.Listing listing in WorldManager.GetSaves())
        {
            //NOTE: This line below is a backup in case the CanExecute commands for the Start/Manage Server buttons don't end up working out
            //if (listing.Version < SaveDataUpgrade.MinimumSaveVersion || listing.Version > NitroxEnvironment.Version) continue;
            
            ServerConfig server = ServerConfig.Load(Path.Combine(WorldManager.SavesFolderDir, listing.Name));
            Servers.Add(new ServerEntry
            {
                Name = server.SaveName,
                Password = server.ServerPassword,
                Seed = server.Seed,
                GameMode = server.GameMode,
                DefaultPlayerPerm = server.DefaultPlayerPerm,
                AutoSaveInterval = server.SaveInterval/1000,
                MaxPlayers = server.MaxConnections,
                Port = server.ServerPort,
                AutoPortForward = server.AutoPortForward,
                AllowLanDiscovery = server.LANDiscoveryEnabled,
                AllowCommands = !server.DisableConsole,
                IsNewServer = false
            });
        }
    }

    private void AddServer(string name, ServerGameMode gameMode)
    {
        Servers.Insert(0, new ServerEntry
        {
            Name = name,
            GameMode = gameMode
        });
    }
}
