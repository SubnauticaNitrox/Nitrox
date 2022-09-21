using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NitroxLauncher.Pages;
using NitroxModel;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store;
using DiscordStore = NitroxModel.Platforms.Store.Discord;
using NitroxModel.Platforms.Store.Interfaces;
using NitroxLauncher.Models.Patching;
using System.Windows;
using System.Windows.Threading;
using NitroxLauncher.Models.Utils;
using System.Windows.Controls;
using System.Diagnostics;

namespace NitroxLauncher
{
    internal class LauncherLogic : IDisposable
    {
        public static string ReleasePhase => NitroxEnvironment.ReleasePhase.ToUpper();
        public static string Version => NitroxEnvironment.Version.ToString();

        public static LauncherLogic Instance { get; private set; }
        public static LauncherConfig Config { get; private set; }
        public static ServerLogic Server { get; private set; }

        private NitroxEntryPatch nitroxEntryPatch;
        private ProcessEx gameProcess;

        private Task<string> lastFindSubnauticaTask;

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
            Server?.Dispose();
            LauncherNotifier.Shutdown();
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
                    Config.IsUpToDate = false;
                    Log.Info($"A new version of the mod ({latestVersion}) is available");

                    LauncherNotifier.Warning($"A new version of the mod ({latestVersion}) is available", new ToastNotifications.Core.MessageOptions()
                    {
                        NotificationClickAction = (n) =>
                        {
                            NavigateTo<UpdatePage>();
                        },
                    });
                }

            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [Conditional("RELEASE")]
        public async void ConfigureFirewall()
        {
            Task task = Task.Run(() => WindowsHelper.CheckFirewallRules());
            await task;

            if (task.Exception != null)
            {
                MessageBox.Show($"An error occurred configuring the firewall: {task.Exception}");
            }
        }

        public async Task<string> SetTargetedSubnauticaPath(string path)
        {
            if (Config.SubnauticaPath == path || !Directory.Exists(path))
            {
                return null;
            }

            Config.SubnauticaPath = path;

            if (lastFindSubnauticaTask != null)
            {
                await lastFindSubnauticaTask;
            }

            lastFindSubnauticaTask = Task.Factory.StartNew(() =>
            {
                PirateDetection.TriggerOnDirectory(path);

                if (!FileSystem.Instance.IsWritable(Directory.GetCurrentDirectory()) || !FileSystem.Instance.IsWritable(path))
                {
                    // TODO: Move this check to another place where Nitrox installation can be verified. (i.e: another page on the launcher in order to check permissions, network setup, ...)
                    if (!FileSystem.Instance.SetFullAccessToCurrentUser(Directory.GetCurrentDirectory()) || !FileSystem.Instance.SetFullAccessToCurrentUser(path))
                    {
                        Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show(Application.Current.MainWindow!, "Restart Nitrox Launcher as admin to allow Nitrox to change permissions as needed. This is only needed once. Nitrox will close after this message.", "Required file permission error", MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                            Environment.Exit(1);
                        }, DispatcherPriority.ApplicationIdle);
                    }
                }
                
                // Save game path as preferred for future sessions.
                NitroxUser.PreferredGamePath = path;
                
                if (nitroxEntryPatch?.IsApplied == true)
                {
                    nitroxEntryPatch.Remove();
                }
                nitroxEntryPatch = new NitroxEntryPatch(() => Config.SubnauticaPath);

                if (Path.GetFullPath(path).StartsWith(WindowsHelper.ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    WindowsHelper.RestartAsAdmin();
                }

                return path;
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

            return await lastFindSubnauticaTask;
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
            nitroxEntryPatch.Remove();
            gameProcess = await StartSubnauticaAsync();
        }

        internal async Task StartMultiplayerAsync()
        {
            if (string.IsNullOrWhiteSpace(Config.SubnauticaPath) || !Directory.Exists(Config.SubnauticaPath))
            {
                NavigateTo<OptionPage>();
                throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
            }

            if (Config.IsPirated)
            {
                throw new Exception("Aarrr! Nitrox walked the plank :(");
            }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif

            // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory 
            string initDllName = "NitroxPatcher.dll";
            try
            {
                File.Copy(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "lib", initDllName),
                    Path.Combine(Config.SubnauticaPath, "Subnautica_Data", "Managed", initDllName),
                    true
                );
            }
            catch (IOException ex)
            {
                Log.Error(ex, "Unable to move initialization dll to Managed folder. Still attempting to launch because it might exist from previous runs.");
            }

            // Try inject Nitrox into Subnautica code.
            if (lastFindSubnauticaTask != null)
            {
                await lastFindSubnauticaTask;
            }
            if (nitroxEntryPatch == null)
            {
                throw new Exception("Nitrox was blocked by another program");
            }
            nitroxEntryPatch.Remove();
            nitroxEntryPatch.Apply();

            if (QModHelper.IsQModInstalled(Config.SubnauticaPath))
            {
                Log.Warn("Seems like QModManager is Installed");
                LauncherNotifier.Info("Detected QModManager in the game folder");
            }

            gameProcess = await StartSubnauticaAsync();
        }

        private async Task<ProcessEx> StartSubnauticaAsync()
        {
            string subnauticaPath = Config.SubnauticaPath;
            string subnauticaLaunchArguments = Config.SubnauticaLaunchArguments;
            string subnauticaExe = Path.Combine(subnauticaPath, GameInfo.Subnautica.ExeName);
            IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(subnauticaPath);
            
            // Start game & gaming platform if needed.
            using ProcessEx game = platform switch
            {
                Steam s => await s.StartGameAsync(subnauticaExe, GameInfo.Subnautica.SteamAppId, subnauticaLaunchArguments),
                EpicGames e => await e.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
                MSStore m => await m.StartGameAsync(subnauticaExe),
                DiscordStore d => await d.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
                _ => throw new Exception($"Directory '{subnauticaPath}' is not a valid {GameInfo.Subnautica.Name} game installation or the game's platform is unsupported by Nitrox.")
            };

            return game ?? throw new Exception($"Unable to start game through {platform.Name}");
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
