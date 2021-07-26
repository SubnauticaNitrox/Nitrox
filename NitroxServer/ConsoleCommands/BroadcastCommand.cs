using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    internal class BroadcastCommand : Command
    {
        private readonly PlayerManager playerManager;

        public BroadcastCommand(PlayerManager playerManager) : base("broadcast", Perms.CONSOLE, "{message}", "Broadcasts a message on the server", new[] {"say"})
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            string joinedArgs = string.Join(" ", args);

            ushort senderId = sender.HasValue ? sender.Value.Id : ChatMessage.SERVER_ID;
            playerManager.SendPacketToAllPlayers(new ChatMessage(senderId, joinedArgs));

            Log.Info("BROADCAST: " + joinedArgs);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length > 0;
        }
    }
}
