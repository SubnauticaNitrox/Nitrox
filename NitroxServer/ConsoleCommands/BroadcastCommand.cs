using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class BroadcastCommand : Command
    {
        private readonly PlayerManager playerManager;

        public BroadcastCommand(PlayerManager playerManager) : base("broadcast", Perms.ADMIN, "Broadcasts a message on the server", true)
        {
            this.playerManager = playerManager;
            AddAlias("say");
            AddParameter(new TypeString("message", true));
        }

        protected override void Execute(Optional<Player> sender)
        {
            ushort senderId = sender.HasValue ? sender.Value.Id : ChatMessage.SERVER_ID;
            string joinedArgs = GetArgOverflow(-1);
            
            playerManager.SendPacketToAllPlayers(new ChatMessage(senderId, joinedArgs));

            Log.Info("BROADCAST: " + joinedArgs);
        }
    }
}
