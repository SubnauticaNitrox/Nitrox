using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels
{
    public class PlayViewModel : RoutableViewModelBase
    {
        public string Greeting => "Hi, I'm the first view!";

        public PlayViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }
    }
}
