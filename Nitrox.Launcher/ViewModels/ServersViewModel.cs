using System.Collections.Generic;
using System.Windows.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels
{
    public class ServersViewModel : RoutableViewModelBase
    {
        public List<ServerEntry> Servers { get; } = new();

        public ServersViewModel(IScreen hostScreen) : base(hostScreen)
        {
            // TODO: Load servers from file
        }

        public class ServerEntry : ReactiveObject
        {
            public string Name { get; set; }
            private bool isOnline;

            public bool IsOnline
            {
                get => isOnline;
                set => this.RaiseAndSetIfChanged(ref isOnline, value);
            }
            public string GameMode { get; set; } = "Survival";
            public int Players { get; set; }
            public int MaxPlayers { get; set; } = 100;
            public ICommand StopCommand { get; init; }
            public ICommand ManageCommand { get; init; }

            public ServerEntry()
            {
                StopCommand = ReactiveCommand.Create(() => IsOnline = false);
                ManageCommand = ReactiveCommand.Create(() => { });
            }
        }
    }
}
