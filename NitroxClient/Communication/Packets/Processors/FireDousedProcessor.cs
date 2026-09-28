using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireDousedProcessor(Fires fires) : IClientPacketProcessor<FireDoused>
{
    private readonly Fires fires = fires;

    public Task Process(ClientProcessorContext context, FireDoused packet)
    {
        fires.Douse(packet.Id, packet.Health, packet.SessionId, packet.DouseRate, packet.OneShot);
        return Task.CompletedTask;
    }
}
