using System;
using System.Diagnostics;
using System.Threading;

namespace NitroxServer
{
    public class ConsoleWindow
    {
        private static Process subnautica;
        private static Process cmd;

        public ConsoleWindow()
        {
            subnautica = Process.GetCurrentProcess();

            Thread thread = new Thread(ConsoleWindow.Start);
            thread.Start();
        }

        public static void Start()
        {
            cmd = new Process
            {
                StartInfo = new ProcessStartInfo("cmd")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };

            cmd.Start();

            while (true)
            {
                if (cmd.HasExited)
                {
                    subnautica.Kill();
                    return;
                }

                Thread.Sleep(500);
            }
        }
    }
}
