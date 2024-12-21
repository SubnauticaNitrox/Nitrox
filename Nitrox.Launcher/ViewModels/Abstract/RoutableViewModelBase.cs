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
    ///     Loads content that the view should show. While the returned task is running a loading indicator will be visible.
    /// </summary>
    internal virtual Task ViewContentLoadAsync() => Task.CompletedTask;

    internal virtual Task ViewContentUnloadAsync() => Task.CompletedTask;
}
