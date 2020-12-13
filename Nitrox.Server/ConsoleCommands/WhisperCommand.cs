using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;

namespace Nitrox.Server.ConsoleCommands
{
    internal class WhisperCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "m", "whisper", "w" };

        public WhisperCommand() : base("msg", Perms.PLAYER, "Sends a private message to a player", true)
        {
            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypeString("msg", true));
        }

        protected override void Execute(CallArgs args)
        {
            Player foundPlayer = args.Get<Player>(0);

            if (foundPlayer != null)
            {
                string message = $"[{args.SenderName} -> YOU]: {args.GetTillEnd(1)}";
                SendMessageToPlayer(foundPlayer, message);
            }
            else
            {
                SendMessage(args.Sender, "Unable to whisper, player not found.");
            }
        }
    }
}
