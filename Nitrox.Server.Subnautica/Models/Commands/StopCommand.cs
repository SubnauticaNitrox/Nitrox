using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class StopCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "exit", "halt", "quit", "close" };

        public StopCommand() : base("stop", Perms.ADMIN, "Stops the server")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Server server = Server.Instance;
            if (!server.IsRunning)
            {
                return;
            }
            SendMessageToAllPlayers("Server is shutting down...");
            server.Stop(shouldSave: true);
        }
    }
}
