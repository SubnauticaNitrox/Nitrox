using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class EmptyServersView : RoutableViewBase<ServersViewModel>
{
    public EmptyServersView()
    {
        InitializeComponent();
    }

    protected override void RegisterDispose(Action<IDisposable> disposables)
    {
    }
}
