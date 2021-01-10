﻿using System.Diagnostics;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
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

            server.Stop();
            using Process proc = Process.Start(program);
        }
    }
}
