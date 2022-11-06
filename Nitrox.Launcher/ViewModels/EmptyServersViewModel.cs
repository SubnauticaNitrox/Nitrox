using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels
{
    public class EmptyServersViewModel : RoutableViewModelBase
    {
        public string Greeting => "Hi, I'm server view!";

        public EmptyServersViewModel(IScreen hostScreen) : base(hostScreen)
        {
        }
    }
}
