﻿using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;

namespace NitroxServer.Communication.Packets.Processors.Abstract
{
    public abstract class UnauthenticatedPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, IProcessorContext connection)
        {
            Process((T)packet, (PlayerConnection)connection);
        }

        public abstract void Process(T packet, PlayerConnection connection);
    }
}
