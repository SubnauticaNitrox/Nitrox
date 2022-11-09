using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Collections;
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
                CreateServerViewModel serverCreator = new();
                CreateServerViewModel? result = await MainViewModel.ShowDialog.Handle(serverCreator);
                if (result == null)
                {
                    return;
                    
                }
                Servers.Add(new ServerEntry
                {
                    Name = result.Name
                });
            });
            
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
