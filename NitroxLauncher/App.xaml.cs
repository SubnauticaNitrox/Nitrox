using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using LibZeroTier;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NitroxModel.Logger;
using System.Collections.Generic;

namespace NitroxLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if(e.Args.Length > 1)
            {
                if (e.Args[0].Equals("zerotiermiddleman"))
                {
                    ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
                    switch (e.Args[1])
                    {
                        case "join":
                            JoinNet(e.Args[2]);
                            break;
                        case "get":
                            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
                            File.WriteAllText(e.Args[3], (nets.Count == 1 && (nets[0] ?? new ZeroTierNetwork() { NetworkStatus = "REQUESTING_CONFIGURATION" } ).NetworkStatus == "OK" && (nets[0] ?? new ZeroTierNetwork() { IsConnected = false }).IsConnected == true).ToString());
                            break;
                    }
                    App.Current.Shutdown();
                    return;
                }
            }

            Log.Setup();
            
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
            Log.Error(e.Exception.GetBaseException().ToString()); // Gets the exception that was unhandled, not the "dispatched unhandled" exception.
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
        private void JoinNet(string serverId)
        {
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
            PrivateNetwork.RestartZeroTier();
            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            foreach(ZeroTierNetwork net in nets)
            {
                if(net.NetworkId != serverId)
                    PrivateNetwork.ZeroTierHandler.LeaveNetwork(net.NetworkId);
            }
            nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            if (nets.Count != 1)
                PrivateNetwork.JoinServerAsync(serverId).Wait();
        }
    }
}
