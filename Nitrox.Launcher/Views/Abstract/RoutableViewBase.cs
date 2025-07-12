using Avalonia.Controls;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

internal abstract class RoutableViewBase<TViewModel> : UserControl
    where TViewModel : RoutableViewModelBase;
