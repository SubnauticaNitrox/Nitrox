using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

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

        protected override void Execute(CallArgs args)
        {
            SendMessageToAllPlayers(args.GetTillEnd());
        }
    }
}
