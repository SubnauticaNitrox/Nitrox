using System;
using System.Windows;
using System.Windows.Threading;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //Change the launcher language based on the OS if possible
            Localization.SetupLanguage(Resources, Localization.GetCurrentCultureName());
            base.OnStartup(e);
        }

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

            Log.Error(e.Exception.GetBaseException().ToString());
            MessageBox.Show(GetExceptionError(e.Exception), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string GetExceptionError(Exception e)
        {
#if RELEASE
            return e.GetBaseException().Message;
#else
            return e.GetBaseException().ToString();
#endif
        }
    }
}
