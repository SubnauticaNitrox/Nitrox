using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class ModalBase<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : ModalViewModelBase
{
    protected ModalBase()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}
