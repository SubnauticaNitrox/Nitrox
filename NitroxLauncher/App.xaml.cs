using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NitroxLauncher.Bootstrap;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    [Serializable]
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            string shadowCopyFolder = "shadowcopy";
            string shadowDir = Path.Combine(Path.GetDirectoryName(entryAssembly.Location), shadowCopyFolder);

            // Restart app in another AppDomain to allow its DLLs to be copied over
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                try
                {
                    Directory.Delete(shadowDir, true);
                }
                catch
                {
                    // ignored
                }

                AppDomainSetup appDomain = new AppDomainSetup();
                appDomain.ShadowCopyFiles = "true";
                appDomain.ApplicationName = entryAssembly.GetName().Name;
                appDomain.ApplicationBase = Directory.GetCurrentDirectory();
                appDomain.ShadowCopyDirectories = AppDomain.CurrentDomain.BaseDirectory;
                appDomain.CachePath = shadowDir;
                appDomain.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                AppDomain domain = AppDomain.CreateDomain("ShadowCopiedLauncher", AppDomain.CurrentDomain.Evidence, appDomain);

                AssemblyLoader loader = (AssemblyLoader)domain.CreateInstanceAndUnwrap(typeof(AssemblyLoader).Assembly.FullName, typeof(AssemblyLoader).FullName);
                loader.LoadAssemlyInfo(entryAssembly.Location);
                AppDomain.Unload(domain);
                return;
            }

            // Set default style for all windows to the style with the target type 'Window' (in App.xaml).
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window),
                                                            new FrameworkPropertyMetadata { DefaultValue = FindResource(typeof(Window)) });
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Page),
                                                            new FrameworkPropertyMetadata { DefaultValue = FindResource(typeof(Page)) });

            // Error if running from a temporary directory because Nitrox Launcher won't be able to write files directly to zip/rar
            // Tools like WinRAR do this to support running EXE files while it's still zipped.
            if (Directory.GetCurrentDirectory().StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Nitrox launcher should not be executed from a temporary directory. Install Nitrox launcher properly by extracting ALL files and moving these to a dedicated location on your PC.",
                                "Invalid working directory",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Environment.Exit(1);
            }

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
