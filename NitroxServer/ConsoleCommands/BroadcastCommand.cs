using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class BroadcastCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "say" };

        public BroadcastCommand() : base("broadcast", Perms.ADMIN, "Broadcasts a message on the server", true)
        {
            AddParameter(new TypeString("message", true));
        }

        protected override void Execute(CallArgs args)
        {
            SendMessageToAllPlayers(args.GetTillEnd());
        }
    }
}
