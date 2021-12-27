using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class RocketPreflightComplete : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual PreflightCheck FlightCheck { get; protected set; }

        private RocketPreflightComplete() { }

        public RocketPreflightComplete(NitroxId id, PreflightCheck flightCheck)
        {
            Id = id;
            FlightCheck = flightCheck;
        }

        public override string ToString()
        {
            return $"[RocketPreflightComplete - Id: {Id}, FlightCheck: {FlightCheck}]";
        }
    }
}
