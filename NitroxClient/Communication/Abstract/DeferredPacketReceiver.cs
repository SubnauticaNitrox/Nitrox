using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public abstract class DeferredPacketReceiver
    {
        public virtual void PacketReceived(Packet packet)
        {
            
        }

        public virtual Queue<Packet> GetReceivedPackets()
        {
            Queue<Packet> packets = new Queue<Packet>();

            return packets;
        }

        internal virtual bool PacketWasDeferred(Packet packet)
        {
            return false;
        }

        internal virtual void AddPacketToDeferredMap(Packet deferred, AbsoluteEntityCell cell)
        {
            
        }
    }
}
