using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DeconstructionCompletedPacketProcessor : AuthenticatedPacketProcessor<DeconstructionCompleted>
    {
        private readonly BaseData baseData;
        private readonly PlayerManager playerManager;
        private readonly BaseSign baseSign;

        public DeconstructionCompletedPacketProcessor(BaseData baseData, PlayerManager playerManager, BaseSign baseSign)
        {
            this.baseData = baseData;
            this.playerManager = playerManager;
            this.baseSign = baseSign;
        }

        public override void Process(DeconstructionCompleted packet, Player player)
        {
            baseData.BasePieceDeconstructionCompleted(packet.Guid);
            playerManager.SendPacketToOtherPlayers(packet, player);

            SignData Sign = baseSign.GetBaseAllSign().SingleOrDefault(x => x.Guid == packet.Guid);

            if (Sign != null)
            {
                baseSign.DeleteBaseSign(packet.Guid);
            }
        }
    }
}
