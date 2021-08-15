using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketPreflightComplete : Packet
    {
        public NitroxId Id { get; }
        public PreflightCheck FlightCheck { get; }

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
