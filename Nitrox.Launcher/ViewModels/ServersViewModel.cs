using System.Windows.Input;
using Avalonia.Collections;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class ServersViewModel : RoutableViewModelBase
{
    public ICommand CreateServerCommand { get; }
    public ICommand ManageServerCommand { get; }
    public AvaloniaList<ServerEntry> Servers { get; } = new();

    public ServersViewModel(IScreen hostScreen) : base(hostScreen)
    {
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
        });

        // TODO: Load servers from file
    }

    private void AddServer(string name, GameMode gameMode)
    {
        Servers.Add(new ServerEntry
        {
            Name = name,
            GameMode = gameMode
        });
    }
}
