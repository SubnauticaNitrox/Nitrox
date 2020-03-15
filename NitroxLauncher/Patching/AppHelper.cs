using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace NitroxLauncher
{
    public class AppHelper
    {
        public static string ProgramFileDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

        public static bool IsAppRunningInAdmin()
        {
            WindowsPrincipal wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdmin(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!IsAppRunningInAdmin())
            {
                if (Path.GetFullPath(path).StartsWith(ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Nitrox launcher should be executed with administrator permissions in order to properly patch Subnautica while in Program Files directory, do you want to restart ?",
                        "Program Files Path Detected",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes,
                        MessageBoxOptions.DefaultDesktopOnly
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Setting up start info of the new process of the same application
                        ProcessStartInfo processStartInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().CodeBase);

                        // Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
                        processStartInfo.UseShellExecute = true;
                        processStartInfo.Verb = "runas";

                        // Start the application as new process
                        Process.Start(processStartInfo);

                        Environment.Exit(1);
                    }

                }
            }
        }

    }
}
