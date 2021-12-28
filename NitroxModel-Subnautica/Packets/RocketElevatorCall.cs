using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class RocketElevatorCall : Packet
    {
        [Index(0)]
        public virtual bool Up { get; protected set; }
        [Index(1)]
        public virtual NitroxId Id { get; protected set; }
        [Index(2)]
        public virtual RocketElevatorPanel Panel { get; protected set; }

        public RocketElevatorCall() { }

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

