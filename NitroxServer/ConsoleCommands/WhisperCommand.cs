using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WhisperCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "w", "msg", "m" };

        public WhisperCommand() : base("whisper", Perms.PLAYER, "Sends a private message to a player")
        {
            AddParameter(new TypePlayer("name", true, "The players name to message"));
            AddParameter(new TypeString("msg", true, "The message to send"));

            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            Player foundPlayer = args.Get<Player>(0);
            string message = $"[{args.SenderName} -> YOU]: {args.GetTillEnd(1)}";

            SendMessageToPlayer(foundPlayer, message);
        }
    }
}
