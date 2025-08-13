using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;
public class CoffeeMachineUseProcessor : AuthenticatedPacketProcessor<CoffeeMachineUse>
{
    private readonly PlayerManager playerManager;
    private readonly float machineRange = 200f;
    public CoffeeMachineUseProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(CoffeeMachineUse packet, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            if (player != sendingPlayer && NitroxVector3.Distance(player.Position, sendingPlayer.Position) < machineRange && player.SubRootId.Equals(sendingPlayer.SubRootId))
            {
                player.SendPacket(packet);
            }
        }
    }
}
