using System;
using Avalonia.Interactivity;
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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Bla");
    }
}
