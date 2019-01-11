using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class SayCommand : Command
    {
        private readonly PlayerManager playerManager;

        public SayCommand(PlayerManager playerManager) : base("say", Perms.Admin, Optional<string>.Of("<message>"), "say Even the lowliest of cogs needs to say something SO SAY SOMETHING!", new[] {"broadcast"})
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Player player)
        {
            if (player.Id != ChatMessage.SERVER_ID)
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "Saying: " + string.Join(" ", args)));
            }
            else
            {
                Log.Info("Saying: " + string.Join(" ", args));
            }
            playerManager.SendPacketToAllPlayers(new ChatMessage(player.Id, string.Join(" ", args)));
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length > 0;
        }
    }
}
