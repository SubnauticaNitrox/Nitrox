using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using NitroxModel.Logger;

namespace NitroxLauncher.Models.Utils
{
    internal static class WindowsHelper
    {
        public static string ProgramFileDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

        internal static bool IsAppRunningInAdmin()
        {
            WindowsPrincipal wp = new(WindowsIdentity.GetCurrent());
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static void RestartAsAdmin()
        {
            if (!IsAppRunningInAdmin())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Nitrox launcher should be executed with administrator permissions in order to properly patch Subnautica while in Program Files directory, do you want to restart ?",
                    "Nitrox needs permissions",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes,
                    MessageBoxOptions.DefaultDesktopOnly
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
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
                    catch (Exception)
                    {
                        Log.Error("Error while trying to instance an admin processus of the launcher, aborting");
                    }
                }
            }
            else
            {
                Log.Info("Can't restart the launcher as admin, we already have the perms");
            }
        }
    }

}
