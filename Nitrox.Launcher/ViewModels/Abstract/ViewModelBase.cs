using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableValidator, IMessageReceiver
{
    protected Window MainWindow => AppViewLocator.MainWindow;

    public virtual void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
