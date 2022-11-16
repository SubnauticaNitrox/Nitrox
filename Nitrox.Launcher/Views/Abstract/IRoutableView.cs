using ReactiveUI;

namespace Nitrox.Launcher.Views.Abstract;

public interface IRoutableView : IViewFor
{
    public object? DataContext { get; set; }
}
