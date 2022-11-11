using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class ModalBase<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class, IModalViewModel
{
    protected ModalBase()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}
