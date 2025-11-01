using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
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
