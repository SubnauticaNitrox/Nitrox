using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerYellProcessor(PlayerYells playerYells) : IClientPacketProcessor<PlayerYell>
{
    private readonly PlayerYells playerYells = playerYells;

    public Task Process(ClientProcessorContext context, PlayerYell packet)
    {
        playerYells.PlayRemote(packet);
        return Task.CompletedTask;
    }
}
