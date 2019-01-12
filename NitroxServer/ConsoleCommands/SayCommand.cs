using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    internal class SayCommand : Command
    {
        private readonly PlayerManager playerManager;

        public SayCommand(PlayerManager playerManager) : base("say", Perms.ADMIN, "<message>", "say Even the lowliest of cogs needs to say something SO SAY SOMETHING!", new[] {"broadcast"})
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            string message = "Saying: " + string.Join(" ", args);
            Log.Info(message);

            if(player.IsPresent())
            {
                playerManager.SendPacketToAllPlayers(new ChatMessage(player.Get().Id, string.Join(" ", args)));
            }
            else
            {
                playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, string.Join(" ", args)));
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length > 0;
        }
    }
}
