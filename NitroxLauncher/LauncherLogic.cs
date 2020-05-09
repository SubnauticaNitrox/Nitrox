using System;
using System.Collections.Generic;
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
        private string subnauticaPath;
        private Process serverProcess;
        private Process gameProcess;
        private bool isEmbedded;

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

        public bool ServerRunning => !serverProcess?.HasExited ?? false;

        public LauncherLogic()
        {
            Instance = this;
        }

        public event EventHandler<ServerStartEventArgs> ServerStarted;
        public event DataReceivedEventHandler ServerDataReceived;
        public event EventHandler ServerExited;

        public void Dispose()
        {
            try
            {
                nitroxEntryPatch.Remove();
            }
            catch (Exception)
            {
                // Ignored
            }

            gameProcess?.Dispose();
            serverProcess?.Dispose();
        }

        public async Task WriteToServerAsync(string inputText)
        {
            if (ServerRunning)
            {
                try
                {
                    await serverProcess.StandardInput.WriteLineAsync(inputText);
                }
                catch (Exception)
                {
                    // Ignore errors while writing to process
                }
            }
        }

        public async void CheckNitroxVersion()
        {
            await Task.Factory.StartNew(() =>
            {
                string latestVersion = WebHelper.GetNitroxLatestVersion();

                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != Version)
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
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NavigateTo(Type page)
        {
            if (page == null || !page.IsSubclassOf(typeof(Page)) && page != typeof(Page))
            {
                return;
            }
            if (ServerRunning && isEmbedded && page == typeof(ServerPage))
            {
                page = typeof(ServerConsolePage);
            }

            if (Application.Current.MainWindow != null)
            {
                ((MainWindow)Application.Current.MainWindow).FrameContent = Application.Current.FindResource(page.Name);
            }
        }

        public void NavigateTo<TPage>() where TPage : Page
        {
            NavigateTo(typeof(TPage));
        }

        public bool NavigationIsOn<TPage>() where TPage : Page
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;
            if (window == null)
            {
                return false;
            }
            return window.FrameContent?.GetType() == typeof(TPage);
        }

        public bool IsSubnauticaDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            return Directory.EnumerateFileSystemEntries(directory, "*.exe")
                .Any(file => Path.GetFileName(file)?.Equals("subnautica.exe", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal async Task StartSingleplayerAsync()
        {
#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
            SyncAssembliesBetweenSubnauticaManagedAndLib();
            nitroxEntryPatch.Remove();
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
            SyncAssetBundles();
            SyncMonoAssemblies();
            SyncAssembliesBetweenSubnauticaManagedAndLib();

            nitroxEntryPatch.Remove(); // Remove any previous instances first.
            nitroxEntryPatch.Apply();

            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        internal Process StartServer(bool standalone)
        {
            if (ServerRunning)
            {
                throw new Exception("An instance of Nitrox Server is already running");
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib();

            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib", "NitroxServer-Subnautica.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo(serverPath);

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

        internal async Task SendServerCommandAsync(string inputText)
        {
            if (!ServerRunning)
            {
                return;
            }

            await WriteToServerAsync(inputText);
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
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = subnauticaPath,
                FileName = subnauticaExe
            };

            if (PlatformDetection.IsEpic(subnauticaPath))
            {
                startInfo.Arguments = "-EpicPortal -vrmode none";
            }
            else if (PlatformDetection.IsSteam(subnauticaPath))
            {
                startInfo.FileName = "steam://run/264710";
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Log.Info("Finished removing patches!");
        }

        private async Task<Process> WaitForProcessAsync()
        {
            if (gameProcess != null)
            {
                return gameProcess;
            }

            return await Task.Run(async () =>
                {
                    int waitTimeInMill = 1000;
                    int maxStartingTimeSeconds = 10;
                    for (int i = 0; i < 1000; i++)
                    {
                        // If wait more than ten seconds, mark game as not starting anymore.
                        // This will not stop the thread. Just if someone closes the launcher before, it will work
                        if (maxStartingTimeSeconds < i * waitTimeInMill)
                        {
                        }
                        Process[] processes = Process.GetProcessesByName("Subnautica");
                        if (processes.Length == 1)
                        {
                            return processes[0];
                        }

                        await Task.Delay(waitTimeInMill);
                    }
                    if (gameProcess == null)
                    {
                        Log.Error("No or multiple subnautica processes found. Cannot remove patches after exited.");
                    }
                    return null;
                })
                .ContinueWith(task =>
                    {
                        Process proc = task.Result;
                        proc.Exited += OnSubnauticaExited;
                        return proc;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void SyncAssembliesBetweenSubnauticaManagedAndLib()
        {
            string subnauticaManagedPath = Path.Combine(subnauticaPath, "Subnautica_Data", "Managed");
            string libDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib");

            List<string> ignoreNitroxBinaries = new List<string>
            {
                "NitroxModel.dll",
                "NitroxServer.dll",
                "NitroxServer-Subnautica.dll",
                "NitroxModel-Subnautica.dll",
                "NitroxPatcher.dll",
                "NitroxClient.dll",
                "0Harmony.dll",
                "Autofac.dll",
                "log4net.dll",
                "protobuf-net.dll",
                "LitJson.dll",
                "dnlib.dll",
                "AssetsTools.NET.dll",
                "LiteNetLib.dll"
            };
            CopyAllAssemblies(subnauticaManagedPath, libDirectory, ignoreNitroxBinaries);

            List<string> ignoreNoBinaries = new List<string>();
            CopyAllAssemblies(libDirectory, subnauticaManagedPath, ignoreNoBinaries);
        }

        private void SyncMonoAssemblies()
        {
            string libDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib");
            string launcherMonoPath = Path.Combine(libDirectory, "Mono");
            string subnauticaMonoPath = Path.Combine(subnauticaPath, "MonoBleedingEdge");

            List<string> ignoreNoBinaries = new List<string>();
            CopyAllAssemblies(launcherMonoPath, subnauticaMonoPath, ignoreNoBinaries);
        }

        private void SyncAssetBundles()
        {
            string NormalizePath(string path)
            {
                return Path.GetFullPath(new Uri(path).LocalPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
            }

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Don't try to sync Asset Bundles if the user placed the launcher in the root of the Subnautica folder.
            if (NormalizePath(currentDirectory) == NormalizePath(subnauticaPath))
            {
                return;
            }

            string subnauticaAssetsPath = Path.Combine(subnauticaPath, "AssetBundles");
            string currentDirectoryAssetsPath = Path.Combine(currentDirectory, "AssetBundles");

            string[] assetBundles = Directory.GetFiles(currentDirectoryAssetsPath);
            Log.Info($"Copying asset files from Launcher directory '{currentDirectoryAssetsPath}' to Subnautica '{subnauticaAssetsPath}'");
            foreach (string assetBundle in assetBundles)
            {
                string from = Path.Combine(currentDirectoryAssetsPath, Path.GetFileName(assetBundle));
                string to = Path.Combine(subnauticaAssetsPath, Path.GetFileName(assetBundle));
                Log.Debug($"Copying asset file '{from}' to '{to}'");
                File.Copy(from, to, true);
            }
        }

        private void CopyAllAssemblies(string source, string destination, List<string> dllsToIgnore)
        {
            foreach (string sourceFilePath in Directory.GetFiles(source))
            {
                string fileName = Path.GetFileName(sourceFilePath);
                if (dllsToIgnore.Contains(fileName))
                {
                    continue;
                }

                string destinationFilePath = Path.Combine(destination, fileName);
                if (File.Exists(destinationFilePath) && fileName.EndsWith("dll"))
                {
                    try
                    {
                        Version sourceVersion = AssemblyName.GetAssemblyName(sourceFilePath).Version;
                        Version destinationVersion = AssemblyName.GetAssemblyName(destinationFilePath).Version;
                        FileInfo destFileInfo = new FileInfo(destinationFilePath);
                        FileInfo sourceFileInfo = new FileInfo(sourceFilePath);

                        if (sourceVersion != destinationVersion || destFileInfo.LastWriteTime != sourceFileInfo.LastWriteTime)
                        {
                            File.Delete(destinationFilePath);
                            File.Copy(sourceFilePath, destinationFilePath, true);
                        }
                    }
                    catch (BadImageFormatException)
                    {
                        // note: discord-rpc.dll has no version information and will fail with BadImageFormatException.
                        // This means the discord-rpc.dll is already present in the destination folder and will be ignored.
                        // Only in case of other dll's the error will be logged.
                        if (!fileName.Equals("discord-rpc.dll"))
                        {
                            Log.Error($"There was an BadImageFormatException determining the version of the assembly: {fileName}");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"There was error during copying the assembly: {fileName}", e);
                    }
                }
                else if (!File.Exists(destinationFilePath))
                {
                    File.Copy(sourceFilePath, destinationFilePath, true);
                }
            }
        }
    }
}
