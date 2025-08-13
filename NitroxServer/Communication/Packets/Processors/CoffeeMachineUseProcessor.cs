using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public sealed class CoffeeMachineUseProcessor : AuthenticatedPacketProcessor<CoffeeMachineUse>
{
    private readonly PlayerManager playerManager;
    public CoffeeMachineUseProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(CoffeeMachineUse packet, Player sendingPlayer)
    {
        playerManager.SendPacketToOtherPlayers(packet, sendingPlayer);
    }
}
