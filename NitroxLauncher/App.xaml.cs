using System.Windows;
using System.Windows.Threading;

namespace NitroxLauncher
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            MainWindow window = (MainWindow)Current.MainWindow;
            window?.CloseInternalServerAndRemovePatchAsync();
            
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // If something went wrong. Close the server
            MainWindow window = (MainWindow)Current.MainWindow;
            window?.CloseInternalServerAndRemovePatchAsync();

            throw e.Exception;
        }
    }
}
