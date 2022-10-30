using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nitrox.Launcher.ViewModels;
using ReactiveUI;

namespace Nitrox.Launcher.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(_ =>
            {
                // TODO: Handle disposables
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
