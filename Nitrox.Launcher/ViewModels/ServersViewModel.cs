using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Messages;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.Serialization.Upgrade;
using NitroxServer.Serialization.World;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class ServersViewModel : RoutableViewModelBase
{
    public AvaloniaList<ServerEntry> Servers { get; } = new();

    public ServersViewModel(IScreen hostScreen) : base(hostScreen)
    {
        WeakReferenceMessenger.Default.Register<ServerEntryPropertyChangedMessage>(this, (sender, message) =>
        {
            if (message.PropertyName == nameof(ServerEntry.IsOnline))
            {
                ManageServerCommand.NotifyCanExecuteChanged();
            }
        });

        // Load servers from the saves folder
        foreach (WorldManager.Listing listing in WorldManager.GetSaves())
        {
            string saveDir = Path.Combine(WorldManager.SavesFolderDir, listing.Name);
            SubnauticaServerConfig server = SubnauticaServerConfig.Load(saveDir);
            Servers.Add(new ServerEntry
            {
                Name = server.SaveName,
                Password = server.ServerPassword,
                Seed = server.Seed,
                GameMode = server.GameMode,
                PlayerPermissions = server.DefaultPlayerPerm,
                AutoSaveInterval = server.SaveInterval/1000,
                MaxPlayers = server.MaxConnections,
                Port = server.ServerPort,
                AutoPortForward = server.AutoPortForward,
                AllowLanDiscovery = server.LANDiscoveryEnabled,
                AllowCommands = !server.DisableConsole,
                IsNewServer = !File.Exists(Path.Combine(saveDir, "WorldData.json")),
                Version = listing.Version
            });
        }
    }

    [RelayCommand]
    public async Task CreateServer()
    {
        CreateServerViewModel result = await MainViewModel.ShowDialogAsync<CreateServerViewModel>();
        if (result == null)
        {
            return;
        }

        AddServer(result.Name, result.SelectedGameMode);
    }

    [RelayCommand(CanExecute = nameof(CanManageServer))]
    public void ManageServer(ServerEntry server)
    {
        ManageServerViewModel viewModel = AppViewLocator.GetSharedViewModel<ManageServerViewModel>();
        viewModel.LoadFrom(server);
        MainViewModel.Router.Navigate.Execute(viewModel);
    }

    private static bool CanManageServer(ServerEntry server) => server.Version >= SaveDataUpgrade.MinimumSaveVersion && server.Version <= NitroxEnvironment.Version;

    private void AddServer(string name, ServerGameMode gameMode)
    {
        Servers.Insert(0, new ServerEntry
        {
            Name = name.Trim(),
            GameMode = gameMode,
            Seed = ""
        });
    }
}
