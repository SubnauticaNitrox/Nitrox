using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class ManageServerView : RoutableViewBase<ManageServerViewModel>
{
    public ManageServerView()
    {
        InitializeComponent();
    }

    protected override void RegisterDispose(Action<IDisposable> disposables)
    {
    }
}
