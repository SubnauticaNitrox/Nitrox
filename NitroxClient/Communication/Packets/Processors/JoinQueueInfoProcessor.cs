using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class JoinQueueInfoProcessor : ClientPacketProcessor<JoinQueueInfo>
{
    public override void Process(JoinQueueInfo packet)
    {
        Log.InGame(Language.main.Get("Nitrox_QueueInfo")
            .Replace("{POSITION}", packet.Position.ToString())
            .Replace("{TIME}", TimeSpan.FromMilliseconds(packet.Timeout * packet.Position).ToString(@"mm\:ss")));
    }
}
