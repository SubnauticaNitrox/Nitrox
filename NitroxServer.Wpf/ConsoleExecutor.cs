using System;
using System.Diagnostics;

namespace NitroxServer.Wpf
{
    /// <summary>
    ///     Executes and handles a console program. Enabling for receiving and sending text.
    /// </summary>
    public class ConsoleExecutor : IDisposable
    {
        public bool IsProcessAlive => Process == null || !Process.HasExited;

        private Process Process { get; }

        private ConsoleExecutor(string filePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = filePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            Process.OutputDataReceived += ProcessOutputDataReceived;
            Process.ErrorDataReceived += ProcessOnErrorDataReceived;

            Process.Start();
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
        }

        /// <summary>
        ///     Executes a program given its path.
        /// </summary>
        /// <param name="filePath">Path to the executable to run.</param>
        /// <returns>A manager that interacts with the given executable when it's running.</returns>
        public static ConsoleExecutor Execute(string filePath)
        {
            return new ConsoleExecutor(filePath);
        }

        public event EventHandler<ProgramOutputReceivedEventArgs> OutputReceived;
        public event EventHandler<ProgramOutputReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Closes 
        /// </summary>
        public void CloseProcess()
        {
            if (!IsProcessAlive)
            {
                return;
            }

            WriteLine("exit");
            Process.CloseMainWindow();
            Process.WaitForExit(5000);
            if (!Process.HasExited)
            {
                Process.Kill();
            }
            Process.Dispose();
        }

        public void Kill()
        {
            if (!IsProcessAlive)
            {
                return;
            }

            Process.Kill();
        }

        public void WriteLine(string text)
        {
            if (!IsProcessAlive)
            {
                return;
            }

            Process.StandardInput.WriteLine(text);
        }

        public void Dispose()
        {
            Process?.Dispose();
        }

        protected virtual void OnOutputReceived(ProgramOutputReceivedEventArgs e)
        {
            OutputReceived?.Invoke(this, e);
        }

        protected virtual void OnErrorReceived(ProgramOutputReceivedEventArgs e)
        {
            ErrorReceived?.Invoke(this, e);
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnErrorReceived(new ProgramOutputReceivedEventArgs(e.Data));
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutputReceived(new ProgramOutputReceivedEventArgs(e.Data));
        }
    }
}
