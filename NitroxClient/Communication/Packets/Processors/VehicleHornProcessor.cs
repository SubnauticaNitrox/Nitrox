using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class VehicleHornProcessor(VehicleHorns vehicleHorns) : IClientPacketProcessor<VehicleHorn>
{
    private readonly VehicleHorns vehicleHorns = vehicleHorns;

    public Task Process(ClientProcessorContext context, VehicleHorn packet)
    {
        vehicleHorns.PlayRemoteHorn(packet.VehicleId);
        return Task.CompletedTask;
    }
}
