using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NitroxLauncher.Models.Events;

namespace NitroxLauncher
{
    public sealed class ServerLogic : IDisposable
    {
        public bool IsServerRunning => !serverProcess?.HasExited ?? false;
        public bool IsEmbedded { get; private set; }

        public event EventHandler<ServerStartEventArgs> ServerStarted;
        public event DataReceivedEventHandler ServerDataReceived;
        public event EventHandler ServerExited;

        private Process serverProcess;

        private bool isEmbedded;

        public void Dispose()
        {
            if (isEmbedded)
            {
                SendServerCommand("stop\n");
            }

            serverProcess?.Dispose();
            serverProcess = null;
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

            try
            {
                serverProcess.StandardInput.WriteLine(inputText);
            }
            catch (Exception)
            {
                // Ignore errors while writing to process
            }
        }

        private void OnEndServer()
        {
            ServerExited?.Invoke(serverProcess, new EventArgs());
            isEmbedded = false;
        }

        private void OnStartServer(bool embedded)
        {
            isEmbedded = embedded;
            ServerStarted?.Invoke(serverProcess, new ServerStartEventArgs(embedded));
        }

        private void ServerProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ServerDataReceived?.Invoke(sender, e);
        }
    }
}
