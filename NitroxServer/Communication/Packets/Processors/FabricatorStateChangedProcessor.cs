using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class FabricatorStateChangedProcessor : AuthenticatedPacketProcessor<FabricatorStateChanged>
{
    private readonly PlayerManager playerManager;

    public FabricatorStateChangedProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(FabricatorStateChanged packet, Player sendingPlayer)
    {
        playerManager.SendPacketToOtherPlayers(packet, sendingPlayer);
    }
}
