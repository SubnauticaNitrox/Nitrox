using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NitroxLauncher.Models.Events;

namespace NitroxLauncher
{
    internal sealed class ServerLogic : IDisposable
    {
        public const string SERVER_EXECUTABLE = "NitroxServer-Subnautica.exe";

        public bool IsManagedByLauncher => IsEmbedded && IsServerRunning;
        public bool IsServerRunning => !serverProcess?.HasExited ?? false;
        public bool IsEmbedded { get; private set; }

        public event EventHandler<ServerStartEventArgs> ServerStarted;
        public event DataReceivedEventHandler ServerDataReceived;
        public event EventHandler ServerExited;

        private Process serverProcess;

        public void Dispose()
        {
            if (IsEmbedded)
            {
                SendServerCommand("stop\n");
            }

            serverProcess?.Dispose();
            serverProcess = null;
        }

        internal Process StartServer(bool standalone, string saveDir)
        {
            if (IsServerRunning)
            {
                throw new Exception("An instance of Nitrox Server is already running");
            }

            string launcherDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string serverPath = Path.Combine(launcherDir, SERVER_EXECUTABLE);
            ProcessStartInfo startInfo = new(serverPath);
            startInfo.WorkingDirectory = launcherDir;

            if (!standalone)
            {
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.CreateNoWindow = true;
            }

            startInfo.Arguments = $@"""{saveDir}""";

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

            try
            {
                serverProcess.StandardInput.WriteLine(inputText);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnEndServer()
        {
            IsEmbedded = false;
            ServerExited?.Invoke(serverProcess, new EventArgs());
        }

        private void OnStartServer(bool embedded)
        {
            IsEmbedded = embedded;
            ServerStarted?.Invoke(serverProcess, new ServerStartEventArgs(embedded));
        }

        private void ServerProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ServerDataReceived?.Invoke(sender, e);
        }
    }
}
