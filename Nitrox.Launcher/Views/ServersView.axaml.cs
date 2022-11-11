using System;
using Avalonia.Markup.Xaml;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class ServersView : RoutableViewBase<ServersViewModel>
{
    public ServersView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void RegisterDispose(Action<IDisposable> disposables)
    {
    }
}
