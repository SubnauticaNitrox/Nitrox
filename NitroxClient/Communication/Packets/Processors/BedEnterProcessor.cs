using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

class BedEnterProcessor : IClientPacketProcessor<BedEnter>
{
    public Task Process(IPacketProcessContext context, BedEnter packet)
    {
        return Task.CompletedTask;
    }
}
