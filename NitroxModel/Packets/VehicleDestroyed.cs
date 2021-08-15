using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleDestroyed : Packet
    {
        public NitroxId Id { get; }
        public string PlayerName { get; }
        public bool GetPilotingMode { get; }

        public VehicleDestroyed(NitroxId id, string playerName, bool getPilotingMode)
        {
            Id = id;
            PlayerName = playerName;
            GetPilotingMode = getPilotingMode;
        }
    }
}
