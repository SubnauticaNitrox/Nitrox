using System.CodeDom;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class UnBanIpCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public UnBanIpCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("unban", Perms.MODERATOR, "unban a player from the server", true)
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            AddParameter(new TypePlayer("name/ip", true));
        }

        protected override void Execute(CallArgs args)
        {
            System.Net.IPAddress temp;
            if (!System.Net.IPAddress.TryParse(args.Get(0), out temp))
            {
                Player player = args.Get<Player>(0);
                Banning.IpBanning.RemoveBan(player.connection.Endpoint.Address.ToString());
                SendMessage(args.Sender, $"The player {player.Name} has been unbanned");
            }
            else
            {
                Banning.IpBanning.RemoveBan(temp.ToString());
            }
        }
    }
}
