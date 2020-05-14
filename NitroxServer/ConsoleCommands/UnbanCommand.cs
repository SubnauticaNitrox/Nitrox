using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.ConsoleCommands
{
    internal class UnbanCommand : Command
    {
        private readonly PlayerManager playerManager;

        public UnbanCommand(PlayerManager playerManager) : base("unban", Perms.ADMIN, "Unbans a player from the server")
        {
            this.playerManager = playerManager;

            AddParameter(new TypePlayer("name", true, false));
        }

        protected override void Execute(CallArgs args)
        {
            Player playerToBan = args.Get<Player>(0);
            playerToBan.IsPlayerBanned = false;
            SendMessage(args.Sender, $"The player {playerToBan.Name} has been unbanned");
        }
    }
}
