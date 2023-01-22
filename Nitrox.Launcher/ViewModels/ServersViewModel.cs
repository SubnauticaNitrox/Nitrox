using System.Windows.Input;
using Avalonia.Collections;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels
{
    public class ServersViewModel : RoutableViewModelBase
    {
        public ICommand CreateServerCommand { get; }
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
                Servers.Add(new ServerEntry { Name = result.Name, GameMode = result.SelectedGameMode });
            });

            // TODO: Load servers from file
        }

        public class ServerEntry : ReactiveObject
        {
            private bool isOnline;
            public string Name { get; set; } = "";

            public bool IsOnline
            {
                get => isOnline;
                set => this.RaiseAndSetIfChanged(ref isOnline, value);
            }

            public GameMode GameMode { get; set; }
            public int Players { get; set; }
            public int MaxPlayers { get; set; } = 100;
            public ICommand StopCommand { get; }
            public ICommand ManageCommand { get; }

            public ServerEntry()
            {
                StopCommand = ReactiveCommand.Create(() => IsOnline = false);
                ManageCommand = ReactiveCommand.Create(() => { });
            }
        }
    }
}
