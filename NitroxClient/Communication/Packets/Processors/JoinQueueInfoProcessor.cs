using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class JoinQueueInfoProcessor : ClientPacketProcessor<JoinQueueInfo>
{
    public override void Process(JoinQueueInfo packet)
    {
        Log.InGame($"You are at position #{packet.Position} in the queue.");

        if (packet.ShowMaximumWait)
        {
            Log.InGame($"The maximum wait time per person is {MillisToMinutes(packet.Timeout)} minutes.");
        }
    }

    private static string MillisToMinutes(int milliseconds)
    {
        double minutes = milliseconds / 60000.0;
        return Math.Round(minutes, 1).ToString();
    }
}
