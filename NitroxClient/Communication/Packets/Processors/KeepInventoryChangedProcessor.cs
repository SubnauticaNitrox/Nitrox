using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class KeepInventoryChangedProcessor(LocalPlayer localPlayer) : IClientPacketProcessor<KeepInventoryChanged>
{
    private readonly LocalPlayer localPlayer = localPlayer;

    public Task Process(IPacketProcessContext context, KeepInventoryChanged packet)
    {
        localPlayer.KeepInventoryOnDeath = packet.KeepInventoryOnDeath;
        return Task.CompletedTask;
    }
}
