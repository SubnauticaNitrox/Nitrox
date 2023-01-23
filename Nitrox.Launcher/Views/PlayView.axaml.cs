using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class PlayView : RoutableViewBase<PlayViewModel>
{
    public PlayView()
    {
        InitializeComponent();
    }

    protected override void RegisterDispose(Action<IDisposable> disposables)
    {
    }
}
