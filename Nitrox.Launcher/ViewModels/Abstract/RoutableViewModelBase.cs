using System.Reactive.Disposables;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Design;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class RoutableViewModelBase : ViewModelBase
{
    public IRoutingScreen HostScreen { get; } = AppViewLocator.HostScreen;

    protected RoutableViewModelBase()
    {
        this.WhenActivated(disposables => Disposable.Create(this, model => model.ViewContentUnloadAsync()).DisposeWith(disposables));
    }

    /// <summary>
    ///     Add content loading to this method. Before this view is shown, it will first show a loading indicator, up until the
    ///     task completes.
    /// </summary>
    internal virtual Task ViewContentLoadAsync() => Task.CompletedTask;

    internal virtual Task ViewContentUnloadAsync() => Task.CompletedTask;
}
