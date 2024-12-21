using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly SubnauticaServerConfig serverConfig;

        public ListCommand(SubnauticaServerConfig serverConfig, PlayerManager playerManager) : base("list", Perms.PLAYER, "Shows who's online")
        {
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            IList<string> players = playerManager.GetConnectedPlayers().Select(player => player.Name).ToList();

            StringBuilder builder = new($"List of players ({players.Count}/{serverConfig.MaxConnections}):\n");
            builder.Append(string.Join(", ", players));

            SendMessage(args.Sender, builder.ToString());
        }
    }
}
