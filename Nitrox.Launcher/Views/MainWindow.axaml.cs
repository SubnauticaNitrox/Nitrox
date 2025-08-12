using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;

namespace Nitrox.Launcher.Views;

public partial class MainWindow : Abstract.WindowEx<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        PointerPressedEvent.Raised.Subscribe(new AnonymousObserver<(object, RoutedEventArgs)>(args =>
        {
            if (args.Item2 is { Handled: false, Source: Control { Tag: string url } control } && control.Classes.Contains("link"))
            {
                ProcessUtils.OpenUrl(url);
                args.Item2.Handled = true;
            }
        }));
    }
}
