using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NitroxLauncher.Models.Patching;
using NitroxLauncher.Models.Utils;
using NitroxLauncher.Pages;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    internal class LauncherLogic : IDisposable
    {
        public static string Version => Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString();
        public static LauncherLogic Instance { get; private set; }
        public static LauncherConfig Config { get; private set; }
        public static ServerLogic Server { get; private set; }

        public const string RELEASE_PHASE = "ALPHA";

        private NitroxEntryPatch nitroxEntryPatch;
        private Process gameProcess;

        public LauncherLogic()
        {
            Config = new LauncherConfig();
            Server = new ServerLogic();
            Instance = this;
        }

        public void Dispose()
        {
            Application.Current.MainWindow?.Hide();

            try
            {
                nitroxEntryPatch.Remove();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while disposing the launcher");
            }

            gameProcess?.Dispose();
            Server.Dispose();
        }

        [Conditional("RELEASE")]
        public async void CheckNitroxVersion()
        {
            await Task.Factory.StartNew(async () =>
            {
                Version latestVersion = await Downloader.GetNitroxLatestVersion();
                Version currentVersion = new(Version);

                if (latestVersion > currentVersion)
                {
                    MessageBox.Show($"A new version of the mod ({latestVersion}) is available !\n\nPlease check our website to download it",
                        "New version available",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question,
                        MessageBoxResult.OK,
                        MessageBoxOptions.DefaultDesktopOnly);
                }

            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<string> SetTargetedSubnauticaPath(string path)
        {
            if (Config.SubnauticaPath == path || !Directory.Exists(path))
            {
                return null;
            }

            Config.SubnauticaPath = path;

            return await Task.Factory.StartNew(() =>
            {
                PirateDetection.TriggerOnDirectory(path);
                File.WriteAllText("path.txt", path);

                if (nitroxEntryPatch?.IsApplied == true)
                {
                    nitroxEntryPatch.Remove();
                }
                nitroxEntryPatch = new NitroxEntryPatch(path);

                if (Path.GetFullPath(path).StartsWith(WindowsHelper.ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    WindowsHelper.RestartAsAdmin();
                }

                return path;
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void NavigateTo(Type page)
        {
            if (page != null && (page.IsSubclassOf(typeof(Page)) || page == typeof(Page)))
            {
                if (Server.IsManagedByLauncher && page == typeof(ServerPage))
                {
                    page = typeof(ServerConsolePage);
                }

                if (Application.Current.MainWindow != null)
                {
                    ((MainWindow)Application.Current.MainWindow).FrameContent = Application.Current.FindResource(page.Name);
                }
            }
        }

        public void NavigateTo<TPage>() where TPage : Page => NavigateTo(typeof(TPage));

        public bool NavigationIsOn<TPage>() where TPage : Page => Application.Current.MainWindow is MainWindow window && window.FrameContent is TPage;

        internal async Task StartSingleplayerAsync()
        {
#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
            nitroxEntryPatch.Remove();
            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        internal async Task StartMultiplayerAsync()
        {
            if (string.IsNullOrWhiteSpace(Config.SubnauticaPath) || !Directory.Exists(Config.SubnauticaPath))
            {
                NavigateTo<OptionPage>();
                throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
            }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
            // Store path where launcher is in AppData for Nitrox bootstrapper to read
            string nitroxAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox");
            Directory.CreateDirectory(nitroxAppData);
            File.WriteAllText(Path.Combine(nitroxAppData, "launcherpath.txt"), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory 
            string bootloaderName = "Nitrox.Bootloader.dll";
            try
            {
                File.Copy(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lib", bootloaderName),
                    Path.Combine(Config.SubnauticaPath, "Subnautica_Data", "Managed", bootloaderName),
                    true
                );
            }
            catch (IOException ex)
            {
                Log.Error(ex, "Unable to move bootloader dll to Managed folder. Still attempting to launch because it might exist from previous runs.");
            }

            // Remove any previous instances first
            nitroxEntryPatch.Remove();
            nitroxEntryPatch.Apply();

            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        private Process StartSubnautica()
        {
            string subnauticaPath = Config.SubnauticaPath;
            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");

            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = subnauticaPath,
                FileName = subnauticaExe
            };

            switch (Config.SubnauticaPlatform)
            {
                case Platform.EPIC:
                    startInfo.Arguments = "-EpicPortal -vrmode none";
                    break;

                case Platform.STEAM:
                    startInfo.FileName = "steam://run/264710";
                    break;

                case Platform.MICROSOFT:
                    startInfo.FileName = "ms-xbl-38616e6e:\\";
                    break;

                default:
                    Log.Error("Failed to retrieve Platform through PlatformDetection, is it updated ?");
                    throw new Exception("Unable to retrieve the platform that runs Subnautica");
            }

            return Process.Start(startInfo);
        }

        private void OnSubnauticaExited(object sender, EventArgs e)
        {
            try
            {
                nitroxEntryPatch.Remove();
                Log.Info("Finished removing patches!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(ex);
            }
        }

        private async Task<Process> WaitForProcessAsync()
        {
            if (gameProcess != null)
            {
                return gameProcess;
            }

            return await Task.Run(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Process[] processes = Process.GetProcessesByName("Subnautica");

                    if (processes.Length > 0)
                    {
                        //When we have multiple instances, we return the first one and we dispose others
                        if (processes.Length > 1)
                        {
                            processes.Skip(1).ForEach(p => p.Dispose());
                        }

                        return processes[0];
                    }

                    await Task.Delay(millisecondsDelay: 1000);
                }

                if (gameProcess == null)
                {
                    Log.Error("No or multiple subnautica processes found. Cannot remove patches after exited.");
                }

                return null;
            }).ContinueWith(task =>
            {
                Process proc = task.Result;
                proc.Exited += OnSubnauticaExited;
                return proc;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
