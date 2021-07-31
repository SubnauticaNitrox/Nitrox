using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EscapePodRepair : Packet
    {
        public NitroxId Id { get; }

        public EscapePodRepair(NitroxId id)
        {
            Id = id;
        }
    }
}
