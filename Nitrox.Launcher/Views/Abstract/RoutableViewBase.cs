using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class RoutableViewBase<TViewModel> : ReactiveUserControl<TViewModel>
    where TViewModel : RoutableViewModelBase
{
}
