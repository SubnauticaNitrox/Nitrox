using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MultiplayerSessionPolicyProcessor(IMultiplayerSession multiplayerSession) : IClientPacketProcessor<MultiplayerSessionPolicy>
{
    private readonly IMultiplayerSession multiplayerSession = multiplayerSession;

    public Task Process(ClientProcessorContext context, MultiplayerSessionPolicy packet)
    {
        Log.Info("Processing session policy information.");
        multiplayerSession.ProcessSessionPolicy(packet);
        return Task.CompletedTask;
    }
}
