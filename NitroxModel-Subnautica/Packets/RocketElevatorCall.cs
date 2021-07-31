using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketElevatorCall : Packet
    {
        public bool Up { get; }
        public NitroxId Id { get; }
        public RocketElevatorPanel Panel { get; }

        public RocketElevatorCall(NitroxId id, RocketElevatorPanel panel, bool up)
        {
            Panel = panel;
            Id = id;
            Up = up;
        }

        public override string ToString()
        {
            return $"[RocketElevatorCall - RocketId: {Id}, Panel : {Panel}, Up: {Up}]";
        }
    }

    public enum RocketElevatorPanel
    {
        EXTERNAL_PANEL,
        INTERNAL_PANEL
    }
}

