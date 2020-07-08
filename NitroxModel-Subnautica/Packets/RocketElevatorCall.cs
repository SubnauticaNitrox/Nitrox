using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketElevatorCall : Packet
    {
        public NitroxId Id { get; }
        public bool Up { get; }

        public RocketElevatorCall(NitroxId id, bool up)
        {
            Id = id;
            Up = up;
        }

        public override string ToString()
        {
            return $"[RocketElevatorCall - RocketId: {Id}, Up: {Up}]";
        }
    }
}

