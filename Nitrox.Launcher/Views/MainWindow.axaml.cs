using System.Threading.Tasks;
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
            this.WhenActivated(d =>
            {
                // TODO: Handle disposables
                d(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync));
            });
            AvaloniaXamlLoader.Load(this);
        }
        
        private async Task DoShowDialogAsync(InteractionContext<CreateServerViewModel, CreateServerViewModel?> interaction)
        {
            CreateServerModal dialog = new() { DataContext = interaction.Input };
            CreateServerViewModel? result = await dialog.ShowDialog<CreateServerViewModel?>(this);
            interaction.SetOutput(result);
        }
    }
}
