using System;
using Avalonia.Markup.Xaml;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class EmptyServersView : RoutableViewBase<EmptyServersViewModel>
{
    public EmptyServersView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void RegisterDispose(Action<IDisposable> disposables)
    {
    }
}
