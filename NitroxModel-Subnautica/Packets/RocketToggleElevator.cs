using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketToggleElevator : Packet
    {
        public NitroxId Id { get; }
        public bool Up { get; }

        public RocketToggleElevator(NitroxId id, bool up)
        {
            Id = id;
            Up = up;
        }

        public override string ToString()
        {
            return $"[RocketToggleElevator Id: {Id}, Up: {Up}]";
        }
    }
}
