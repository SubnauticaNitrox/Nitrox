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

        public BroadcastCommand(PlayerManager playerManager) : base("broadcast", Perms.CONSOLE, "Broadcasts a message on the server", true)
        {
            this.playerManager = playerManager;
            AddAlias("say");
            AddParameter(TypeString.Get, "message", true);
        }

        protected override void Perform(Optional<Player> sender)
        {
            string joinedArgs = GetArgOverflow(-1);

            ushort senderId = sender.HasValue ? sender.Value.Id : ChatMessage.SERVER_ID;
            playerManager.SendPacketToAllPlayers(new ChatMessage(senderId, joinedArgs));

            Log.Info("BROADCAST: " + joinedArgs);
        }
    }
}
