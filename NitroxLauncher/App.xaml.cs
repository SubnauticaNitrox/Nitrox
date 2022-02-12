global using NitroxModel.Logger;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NitroxLauncher
{
    /// <summary>
    ///     Use MainWindow.xaml.cs or LauncherLogic.cs for Nitrox init code to make exceptions during startup unlikely.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set default style for all windows to the style with the target type 'Window' (in App.xaml).
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(Window))
                });
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Page),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(Page))
                });

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // If something went wrong. Close the server if embedded.
            LauncherLogic.Instance.Dispose();

            Log.Error(e.Exception.GetBaseException().ToString()); // Gets the exception that was unhandled, not the "dispatched unhandled" exception.
            MessageBox.Show(GetExceptionError(e.Exception), "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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