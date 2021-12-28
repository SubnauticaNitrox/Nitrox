using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleDestroyed : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual string PlayerName { get; protected set; }
        [Index(2)]
        public virtual bool GetPilotingMode { get; protected set; }

        public VehicleDestroyed() { }

        public VehicleDestroyed(NitroxId id, string playerName, bool getPilotingMode)
        {
            Id = id;
            PlayerName = playerName;
            GetPilotingMode = getPilotingMode;
        }
    }
}
