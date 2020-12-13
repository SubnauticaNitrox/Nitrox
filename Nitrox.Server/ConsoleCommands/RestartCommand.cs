using System.Diagnostics;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Server.ConsoleCommands.Abstract;

namespace Nitrox.Server.ConsoleCommands
{
    internal sealed class RestartCommand : Command
    {
        private readonly Server server;

        public RestartCommand(Server server) : base("restart", Perms.CONSOLE, "Restarts the server")
        {
            this.server = server;
        }

        protected override void Execute(CallArgs args)
        {
            if (Debugger.IsAttached)
            {
                Log.Error("Cannot restart server while debugger is attached.");
                return;
            }
            string program = Process.GetCurrentProcess().MainModule?.FileName;
            if (program == null)
            {
                Log.Error("Failed to get location of server.");
                return;
            }

            using Process proc = Process.Start(program);
            server.Stop();
        }
    }
}
