using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EscapePodRadioRepair : Packet
    {
        public NitroxId Id { get; }

        public EscapePodRadioRepair(NitroxId id)
        {
            Id = id;
        }
    }
}
