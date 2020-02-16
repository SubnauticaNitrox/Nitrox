using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set default style for all windows to the style with the target type 'Window' (in App.xaml).
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Window))
            });
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Page))
            });
            
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
            Log.Exception(e.Exception.GetBaseException().ToString(), e.Exception);
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
