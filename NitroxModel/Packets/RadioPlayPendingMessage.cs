using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RadioPlayPendingMessage : Packet
    {
        public override string ToString()
        {
            return "[RadioPlayPendingMessage]";
        }
    }
}
