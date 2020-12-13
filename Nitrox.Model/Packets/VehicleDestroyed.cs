using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return "[VehicleRemove - Vehicle Id: " + Id + " PlayerName: " + PlayerName + " GetPilotingMode: " + GetPilotingMode + "]";
        }
    }
}
