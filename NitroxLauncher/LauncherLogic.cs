﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NitroxLauncher.Events;
using NitroxLauncher.Pages;
using NitroxLauncher.Patching;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    public class LauncherLogic : IDisposable, INotifyPropertyChanged
    {
        public static string Version => Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString();
        public static LauncherLogic Instance { get; private set; }
        public const string RELEASE_PHASE = "ALPHA";

        private NitroxEntryPatch nitroxEntryPatch;
        private Process serverProcess;
        private Process gameProcess;
        private bool isEmbedded;

        private string subnauticaPath;
        public string SubnauticaPath
        {
            get => subnauticaPath;
            private set
            {
                value = Path.GetFullPath(value); // Ensures the path looks alright (no mixed / and \ path separators)
                subnauticaPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerRunning => !serverProcess?.HasExited ?? false;

        public LauncherLogic()
        {
            Instance = this;
        }

        public event EventHandler<ServerStartEventArgs> ServerStarted;
        public event DataReceivedEventHandler ServerDataReceived;
        public event EventHandler ServerExited;

        public void Dispose()
        {
            Application.Current.MainWindow?.Hide();
            if (isEmbedded)
            {
                Instance.SendServerCommand("stop\n");
            }

            try
            {
                nitroxEntryPatch.Remove();
                QModHelper.RestoreQModManagerFolders(subnauticaPath);
            }
            catch (Exception)
            {
                // Ignored
            }

            gameProcess?.Dispose();
            serverProcess?.Dispose();
            serverProcess = null; // Indicate the process is dead now.
        }

        public void WriteToServer(string inputText)
        {
            if (IsServerRunning)
            {
                try
                {
                    serverProcess.StandardInput.WriteLine(inputText);
                }
                catch (Exception)
                {
                    // Ignore errors while writing to process
                }
            }
        }

        [Conditional("RELEASE")]
        public async void CheckNitroxVersion()
        {
            await Task.Factory.StartNew(() =>
            {
                Version latestVersion = WebHelper.GetNitroxLatestVersion();
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
            if (SubnauticaPath == path || !Directory.Exists(path))
            {
                return null;
            }

            SubnauticaPath = path;

            return await Task.Factory.StartNew(() =>
            {
                PirateDetection.TriggerOnDirectory(path);
                File.WriteAllText("path.txt", path);

                if (nitroxEntryPatch?.IsApplied == true)
                {
                    nitroxEntryPatch.Remove();
                }
                nitroxEntryPatch = new NitroxEntryPatch(path);

                if (Path.GetFullPath(path).StartsWith(AppHelper.ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    AppHelper.RestartAsAdmin();
                }

                return path;
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void NavigateTo(Type page)
        {
            if (page == null || !page.IsSubclassOf(typeof(Page)) && page != typeof(Page))
            {
                return;
            }

            if (IsServerRunning && isEmbedded && page == typeof(ServerPage))
            {
                page = typeof(ServerConsolePage);
            }

            if (Application.Current.MainWindow != null)
            {
                ((MainWindow)Application.Current.MainWindow).FrameContent = Application.Current.FindResource(page.Name);
            }
        }

        public void NavigateTo<TPage>() where TPage : Page => NavigateTo(typeof(TPage));

        public bool NavigationIsOn<TPage>() where TPage : Page
        {
            if (Application.Current.MainWindow is not MainWindow window)
            {
                return false;
            }

            return window.FrameContent is TPage;
        }

        internal async Task StartSingleplayerAsync()
        {
#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
            nitroxEntryPatch.Remove(); //Merged qmm restore into remove.

            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        internal async Task StartMultiplayerAsync()
        {
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
            
            nitroxEntryPatch.Apply(); //Merged remove and qmm remove into apply.

            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        internal Process StartServer(bool standalone)
        {
            if (IsServerRunning)
            {
                throw new Exception("An instance of Nitrox Server is already running");
            }

            string launcherDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string serverPath = Path.Combine(launcherDir, "NitroxServer-Subnautica.exe");
            ProcessStartInfo startInfo = new(serverPath);
            startInfo.WorkingDirectory = launcherDir;

            if (!standalone)
            {
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.CreateNoWindow = true;
            }

            serverProcess = Process.Start(startInfo);
            if (serverProcess != null)
            {
                serverProcess.EnableRaisingEvents = true; // Required for 'Exited' event from process.

                if (!standalone)
                {
                    serverProcess.OutputDataReceived += ServerProcessOnOutputDataReceived;
                    serverProcess.BeginOutputReadLine();
                }

                serverProcess.Exited += (sender, args) => OnEndServer();
                OnStartServer(!standalone);
            }
            return serverProcess;
        }

        internal void SendServerCommand(string inputText)
        {
            if (!IsServerRunning)
            {
                return;
            }

            WriteToServer(inputText);
        }

        private void OnEndServer()
        {
            ServerExited?.Invoke(serverProcess, new EventArgs());
            isEmbedded = false;
        }

        private void ServerProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ServerDataReceived?.Invoke(sender, e);
        }

        private Process StartSubnautica()
        {
            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = subnauticaPath,
                FileName = subnauticaExe
            };

            switch (PlatformDetection.GetPlatform(SubnauticaPath))
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

        private void OnStartServer(bool embedded)
        {
            isEmbedded = embedded;
            ServerStarted?.Invoke(serverProcess, new ServerStartEventArgs(embedded));
        }

        private void OnSubnauticaExited(object sender, EventArgs e)
        {
            try
            {
                nitroxEntryPatch.Remove();
                Log.Info("Finished removing patches!");
                QModHelper.RestoreQModManagerFolders(subnauticaPath);
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
