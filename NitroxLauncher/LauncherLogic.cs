using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NitroxLauncher.Events;
using NitroxLauncher.Patching;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    public class LauncherLogic : IDisposable
    {
        private readonly NitroxEntryPatch nitroxEntryPatch;
        private readonly string subnauticaPath;
        private Process gameProcess;
        private bool gameStarting;
        private Process serverProcess;

        public bool HasSomethingRunning => ClientRunning || ServerRunning;
        public bool ClientRunning => !gameProcess?.HasExited ?? gameStarting;
        public bool ServerRunning => !serverProcess?.HasExited ?? false;

        private LauncherLogic(string subnauticaPath)
        {
            this.subnauticaPath = subnauticaPath;
            PirateDetection.TriggerOnDirectory(subnauticaPath);
            nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
        }

        public static LauncherLogic Create()
        {
            List<string> errors = new List<string>();
            Optional<string> installation = GameInstallationFinder.Instance.FindGame(errors);

            if (!installation.IsPresent())
            {
                throw new Exception($"Please configure your Subnautica location in settings. Errors:\n{string.Join("\n - ", errors)}");
            }

            return new LauncherLogic(installation.Get());
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

        internal async Task StartSingleplayerAsync()
        {
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return;
            }
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib();
            nitroxEntryPatch.Remove();
            gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
        }

        internal async Task StartMultiplayerAsync()
        {
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return;
            }
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }

            try
            {
                gameStarting = true;

                SyncAssetBundles();
                SyncMonoAssemblies();
                SyncAssembliesBetweenSubnauticaManagedAndLib();

                nitroxEntryPatch.Remove(); // Remove any previous instances first.
                nitroxEntryPatch.Apply();

                gameProcess = StartSubnautica() ?? await WaitForProcessAsync();
            }
            catch (Exception)
            {
                gameStarting = false;
                throw;
            }
        }

        internal Process StartServer(bool standalone)
        {
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return null;
            }
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

        private void OnEndServer()
        {
            ServerExited?.Invoke(serverProcess, new EventArgs());
        }

        private void ServerProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ServerDataReceived?.Invoke(sender, e);
        }

        private Process StartSubnautica()
        {
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return null;
            }

            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = subnauticaPath,
                FileName = subnauticaExe
            };

            if (PlatformDetection.IsEpic(subnauticaPath))
            {
                startInfo.Arguments = "-EpicPortal";
            }
            else if (PlatformDetection.IsSteam(subnauticaPath))
            {
                startInfo.FileName = "steam://run/264710";
            }

            return Process.Start(startInfo);
        }

        private void OnStartServer(bool embedded)
        {
            ServerStarted?.Invoke(serverProcess, new ServerStartEventArgs(embedded));
        }

        private void OnSubnauticaExited(object sender, EventArgs e)
        {
            gameStarting = false;
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
                            gameStarting = false;
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
                        gameStarting = false;
                    }
                    return null;
                })
                .ContinueWith(task =>
                    {
                        gameStarting = false;

                        Process proc = task.Result;
                        proc.Exited += OnSubnauticaExited;
                        return proc;
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void SyncAssembliesBetweenSubnauticaManagedAndLib()
        {
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return;
            }

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

            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return;
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
            if (PirateDetection.TriggerOnDirectory(subnauticaPath))
            {
                return;
            }

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

                        if (sourceVersion != destinationVersion)
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
