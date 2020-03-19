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

        public BroadcastCommand(PlayerManager playerManager) : base("broadcast", Perms.CONSOLE, "<message>", "Broadcasts a message on the server", new[] {"say"})
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            string message = "BROADCAST: " + string.Join(" ", args);

            if(sender.IsPresent())
            {
                playerManager.SendPacketToAllPlayers(new ChatMessage(sender.Get().Id, string.Join(" ", args)));
            }
            else
            {
                playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, string.Join(" ", args)));
            }

            Log.Info(message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length > 0;
        }
    }
}
