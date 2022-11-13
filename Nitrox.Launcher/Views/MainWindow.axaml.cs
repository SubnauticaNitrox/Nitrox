using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views
{
    public partial class MainWindow : WindowBase<MainWindowViewModel>
    {
        public MainWindow()
        {
            // Handle thrown exceptions so they aren't hidden.
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    UnhandledExceptionHandler(ex);
                }
            };
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                if (!args.Observed)
                {
                    UnhandledExceptionHandler(args.Exception);
                }
            };
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(UnhandledExceptionHandler);
            
            RegisterModal<ErrorModal, ErrorViewModel>(() => ViewModel!.ErrorDialog);
            RegisterModal<CreateServerModal, CreateServerViewModel>(() => ViewModel!.CreateServerDialog);
            
            AvaloniaXamlLoader.Load(this);
        }

        private async void UnhandledExceptionHandler(Exception ex)
        {
            await ViewModel!.ErrorDialog.Handle(new(ex));
        }
    }
}
