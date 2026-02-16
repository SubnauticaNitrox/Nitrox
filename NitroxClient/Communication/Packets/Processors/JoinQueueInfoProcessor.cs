using System;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class JoinQueueInfoProcessor : IClientPacketProcessor<JoinQueueInfo>
{
    public Task Process(ClientProcessorContext context, JoinQueueInfo packet)
    {
        Log.InGame(Language.main.Get("Nitrox_QueueInfo")
                           .Replace("{POSITION}", packet.Position.ToString())
                           .Replace("{TIME}", TimeSpan.FromMilliseconds(packet.Timeout * packet.Position).ToString(@"mm\:ss")));
        return Task.CompletedTask;
    }
}
