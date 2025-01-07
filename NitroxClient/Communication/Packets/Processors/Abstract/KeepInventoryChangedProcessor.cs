using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors.Abstract;

public class KeepInventoryChangedProcessor : ClientPacketProcessor<KeepInventoryChanged>
{
    private readonly LocalPlayer localPlayer;

    public KeepInventoryChangedProcessor(LocalPlayer localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    public override void Process(KeepInventoryChanged packet)
    {
        localPlayer.KeepInventoryOnDeath = packet.KeepInventoryOnDeath;
    }
}
