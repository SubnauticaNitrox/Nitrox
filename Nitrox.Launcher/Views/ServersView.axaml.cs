using System;
using System.Diagnostics;
using Avalonia.Input;
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

    private void NitroxWikiTextBlock_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://nitrox.rux.gg/wiki/article/run-and-host-nitrox-subnautica-server") { UseShellExecute = true })?.Dispose();
    }
}
