using Avalonia.Markup.Xaml;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views
{
    public partial class MainWindow : WindowBase<MainWindowViewModel>
    {
        public MainWindow()
        {
            RegisterModal<CreateServerModal, CreateServerViewModel>(() => ViewModel!.CreateServerDialog);
            AvaloniaXamlLoader.Load(this);
        }
    }
}
