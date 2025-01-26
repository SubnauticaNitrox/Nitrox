using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableValidator, IActivatableViewModel
{
    protected Window MainWindow => AppViewLocator.MainWindow;
    public ViewModelActivator Activator { get; } = new();

    protected void RegisterMessageListener<T, TViewModel>(Func<T, TViewModel, Task> asyncFunc) where T : class where TViewModel : ViewModelBase
    {
        if (WeakReferenceMessenger.Default.IsRegistered<T>(this))
        {
            WeakReferenceMessenger.Default.Unregister<T>(this);
        }
        WeakReferenceMessenger.Default.Register<T>(this, (_, message) => asyncFunc(message, (TViewModel)this));
    }

    protected void RegisterMessageListener<T, TViewModel>(Action<T, TViewModel> action) where T : class where TViewModel : ViewModelBase
    {
        if (WeakReferenceMessenger.Default.IsRegistered<T>(this))
        {
            WeakReferenceMessenger.Default.Unregister<T>(this);
        }
        WeakReferenceMessenger.Default.Register<T>(this, (_, message) => action(message, (TViewModel)this));
    }
}
