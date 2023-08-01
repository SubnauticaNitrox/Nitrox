using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nitrox.Launcher.ViewModels.Abstract;

public abstract class ViewModelBase : ObservableValidator
{
    protected Window MainWindow => AppViewLocator.MainWindow;
}
