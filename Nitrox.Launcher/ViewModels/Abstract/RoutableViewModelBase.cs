using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;

namespace Nitrox.Launcher.ViewModels.Abstract;

internal abstract class RoutableViewModelBase : ViewModelBase
{
    /// <summary>
    ///     Updates the current view container to show a different view, as is known by the TViewModel type.
    /// </summary>
    protected void ChangeView<TViewModel>(TViewModel viewModel) where TViewModel : RoutableViewModelBase
    {
        WeakReferenceMessenger.Default.Send(new ShowViewMessage
        {
            ViewModel = viewModel
        });
    }

    protected void ChangeViewToPrevious()
    {
        WeakReferenceMessenger.Default.Send(new ShowPreviousViewMessage());
    }

    protected void ChangeViewToPrevious<T>() where T : RoutableViewModelBase
    {
        WeakReferenceMessenger.Default.Send(new ShowPreviousViewMessage(typeof(T)));
    }

    /// <summary>
    ///     Loads content that the view should show. While the returned task is running a loading indicator will be visible.
    /// </summary>
    internal virtual Task ViewContentLoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    internal virtual Task ViewContentUnloadAsync() => Task.CompletedTask;
}
