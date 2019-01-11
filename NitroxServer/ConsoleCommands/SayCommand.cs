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
            SupportsClientSide = true;
        }

        public override void RunCommand(string[] args)
        {
            playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, string.Join(" ", args)));
        }

        public override void RunCommand(string[] args, Player player)
        {
            playerManager.SendPacketToAllPlayers(new ChatMessage(player.Id, string.Join(" ", args)));
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length > 0;
        }
    }
}
