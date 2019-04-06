using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NitroxLauncher
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // If something went wrong. Close the server
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            window.CloseInternalServerAndRemovePatch();

            throw e.Exception;
        }
    }
}
