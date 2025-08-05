using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Nitrox.Launcher.ViewModels;

namespace Nitrox.Launcher.Views;

internal partial class MainWindow : Abstract.WindowEx<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        PointerPressedEvent.Raised.Subscribe(new AnonymousObserver<(object, RoutedEventArgs)>(args =>
        {
            if (args.Item2 is PointerPressedEventArgs pArgs &&
                pArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed &&
                pArgs.Handled == false &&
                pArgs.Source is Control { Tag: string url } control &&
                control.Classes.Contains("link"))
            {
                OpenUri(url);
                args.Item2.Handled = true;
            }
        }));
    }
}
