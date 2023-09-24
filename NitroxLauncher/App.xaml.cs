using System;
using System.Windows;
using System.Windows.Threading;

namespace NitroxLauncher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
        {
            Exception error = (Exception)e.ExceptionObject;
            Log.Error(error.GetBaseException());

            MessageBox.Show(GetExceptionError(error), "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        };

        DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) =>
        {
            // Gets the exception that was unhandled, not the "dispatched unhandled" exception.
            Log.Error(e.Exception.GetBaseException()); 

            // If something went wrong. Close the server if embedded.
            LauncherLogic.Instance.Dispose();

            MessageBox.Show(GetExceptionError(e.Exception), "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        };

        base.OnStartup(e);
    }

    public static string GetExceptionError(Exception e)
    {
#if RELEASE
        return e.GetBaseException().Message;
#else
        return e.GetBaseException().ToString();
#endif
    }
}
