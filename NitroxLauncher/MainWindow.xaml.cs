using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using NitroxLauncher.Models.Events;
using NitroxLauncher.Models.Properties;
using NitroxLauncher.Pages;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string Version => $"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";

        public object FrameContent
        {
            get => frameContent;
            set
            {
                frameContent = value;
                OnPropertyChanged();
            }
        }

        public bool UpdateAvailable => !LauncherLogic.Config.IsUpToDate;

        private readonly LauncherLogic logic;
        private bool isServerEmbedded;
        private object frameContent;

        private Button LastButton { get; set; }

        public MainWindow()
        {
            Log.Setup();
            LauncherNotifier.Setup();
            
            logic = new LauncherLogic();

            MaxHeight = SystemParameters.VirtualScreenHeight;
            MaxWidth = SystemParameters.VirtualScreenWidth;

            LauncherLogic.Config.PropertyChanged += (s, e) => OnPropertyChanged(nameof(UpdateAvailable));
            LauncherLogic.Server.ServerStarted += ServerStarted;
            LauncherLogic.Server.ServerExited += ServerExited;

            InitializeComponent();

            // Pirate trigger should happen after UI is loaded.
            Loaded += (sender, args) =>
            {
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
                
                // This pirate detection subscriber is immediately invoked if pirate has been detected right now.
                PirateDetection.PirateDetected += (o, eventArgs) =>
                {
                    LauncherNotifier.Info("Nitrox does not support pirated version of Subnautica");
                    LauncherNotifier.Info("Yo ho ho, Ahoy matey! Ye be a pirate!");

                    WebBrowser webBrowser = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0),
                        
                        Height = MinHeight * 0.7,
                        Width = MinWidth * 0.7
                    };

                    FrameContent = webBrowser;

                    webBrowser.NavigateToString($"""
                    <html>
                        <head>
                            <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
                        </head>
                        <body>
                            <iframe width="{webBrowser.Width - 24}" height="{webBrowser.Height - 24}" src="https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1&loop=1&showinfo=0&controls=0" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>
                        </body>
                    </html>
                    """);
                    SideBarPanel.Visibility = Visibility.Hidden;
                };

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    Log.Warn("Launcher may not be connected to internet");
                    LauncherNotifier.Error("Launcher may not be connected to internet");
                }

                if (!NitroxEnvironment.IsReleaseMode)
                {
                    LauncherNotifier.Warning("You're now using Nitrox DEV build");
                }
            };

            logic.SetTargetedSubnauticaPath(NitroxUser.GamePath)
                 .ContinueWith(task =>
                 {
                     if (GameInstallationFinder.IsSubnauticaDirectory(task.Result))
                     {
                         LauncherLogic.Instance.NavigateTo<LaunchGamePage>();
                     }
                     else
                     {
                         LauncherLogic.Instance.NavigateTo<OptionPage>();
                     }

                     logic.CheckNitroxVersion();
                     logic.ConfigureFirewall();
                 }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CanClose()
        {
            if (LauncherLogic.Server.IsServerRunning && isServerEmbedded)
            {
                MessageBoxResult userAnswer = MessageBox.Show("The embedded server is still running. Click yes to close.", "Close aborted", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return userAnswer == MessageBoxResult.Yes;
            }

            return true;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose())
            {
                e.Cancel = true;
                return;
            }

            logic.Dispose();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimze_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            RestoreButton.Visibility = Visibility.Collapsed;
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            isServerEmbedded = e.IsEmbedded;

            if (e.IsEmbedded)
            {
                LauncherLogic.Instance.NavigateTo<ServerConsolePage>();
            }
        }

        private void ServerExited(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                if (LauncherLogic.Instance.NavigationIsOn<ServerConsolePage>())
                {
                    LauncherLogic.Instance.NavigateTo<ServerPage>();
                }
            });
        }

        private void ButtonNavigation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement elem)
            {
                return;
            }
            if (sender is not Button button)
            {
                return;
            }

            LauncherLogic.Instance.NavigateTo(elem.Tag?.GetType());
            if (LastButton == null)
            {
                NavPlayGamePageButton.SetValue(ButtonProperties.SelectedProperty, false);
            }
            else
            {
                LastButton.SetValue(ButtonProperties.SelectedProperty, false);
            }
            LastButton = button;
            button.SetValue(ButtonProperties.SelectedProperty, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// Window Animation Code
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        public IntPtr myHWND;
        public const int GWL_STYLE = -16;

        public static class WS
        {
            public static readonly long
            WS_BORDER = 0x00800000L,
            WS_CAPTION = 0x00C00000L,
            WS_CHILD = 0x40000000L,
            WS_CHILDWINDOW = 0x40000000L,
            WS_CLIPCHILDREN = 0x02000000L,
            WS_CLIPSIBLINGS = 0x04000000L,
            WS_DISABLED = 0x08000000L,
            WS_DLGFRAME = 0x00400000L,
            WS_GROUP = 0x00020000L,
            WS_HSCROLL = 0x00100000L,
            WS_ICONIC = 0x20000000L,
            WS_MAXIMIZE = 0x01000000L,
            WS_MAXIMIZEBOX = 0x00010000L,
            WS_MINIMIZE = 0x20000000L,
            WS_MINIMIZEBOX = 0x00020000L,
            WS_OVERLAPPED = 0x00000000L,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000L,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEBOX = 0x00040000L,
            WS_SYSMENU = 0x00080000L,
            WS_TABSTOP = 0x00010000L,
            WS_THICKFRAME = 0x00040000L,
            WS_TILED = 0x00000000L,
            WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_VISIBLE = 0x10000000L,
            WS_VSCROLL = 0x00200000L;
        }

        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myHWND = new WindowInteropHelper(this).Handle;
            IntPtr myStyle = new IntPtr(WS.WS_CAPTION | WS.WS_CLIPCHILDREN | WS.WS_MINIMIZEBOX | WS.WS_MAXIMIZEBOX | WS.WS_SYSMENU | WS.WS_SIZEBOX);
            SetWindowLongPtr(new HandleRef(null, myHWND), GWL_STYLE, myStyle);
        }
    }
}
