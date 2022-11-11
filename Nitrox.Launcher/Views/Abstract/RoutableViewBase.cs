using System;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class RoutableViewBase<TViewModel> : ReactiveUserControl<TViewModel> where TViewModel : RoutableViewModelBase
{
    protected RoutableViewBase()
    {
        this.WhenActivated(RegisterDispose);
    }

    protected abstract void RegisterDispose(Action<IDisposable> disposables);
}
