using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableObject
{
    protected Window MainWindow => AppViewLocator.MainWindow;
}
