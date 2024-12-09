using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableValidator, IActivatableViewModel
{
    protected Window MainWindow => AppViewLocator.MainWindow;
    public ViewModelActivator Activator { get; } = new();
}
