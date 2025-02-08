using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.Models;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableValidator, IActivatableViewModel, IMessageReceiver
{
    protected Window MainWindow => AppViewLocator.MainWindow;
    public ViewModelActivator Activator { get; } = new();
    public virtual void Dispose() => Activator.Dispose();
}
