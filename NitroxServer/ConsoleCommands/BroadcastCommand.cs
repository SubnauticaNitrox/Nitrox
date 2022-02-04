using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class BroadcastCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "say" };

        public BroadcastCommand() : base("broadcast", Perms.MODERATOR, "Broadcasts a message on the server")
        {
            AddParameter(new TypeString("message", true, "The message to be broadcast"));

            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            SendMessageToAllPlayers(args.GetTillEnd());
        }
    }
}
