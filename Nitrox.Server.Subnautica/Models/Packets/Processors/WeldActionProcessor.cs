using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WeldActionProcessor(SimulationOwnershipData simulationOwnershipData) : IAuthPacketProcessor<WeldAction>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public async Task Process(AuthProcessorContext context, WeldAction packet)
    {
        // TODO: Fix getting player lock for entity ownership.
        // NitroxServer.Player simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);

        // if (simulatingPlayer != null)
        // {
        //     Log.Debug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
        //     simulatingPlayer.SendPacket(packet);
        // }
    }
}
