using System;
using System.ComponentModel;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel, IActivatableViewModel
{
    /// <summary>
    ///     Gets the unique URL for the view.
    /// </summary>
    public string UrlPathSegment => Convert.ToHexString(GetType().Name.AsMd5Hash());

    public IScreen HostScreen { get; }

    protected RoutableViewModelBase(IScreen screen)
    {
        HostScreen = screen;
    }

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);

    /// <summary>
    ///     Pass-through event from MVVM toolkit to ReactiveUI.
    /// </summary>
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

    public ViewModelActivator Activator { get; } = new();
}
