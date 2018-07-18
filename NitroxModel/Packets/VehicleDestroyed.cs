using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleDestroyed : Packet
    {
        public string Guid { get; }
        public string PlayerName { get; }
        public bool GetPilotingMode { get; }

        public VehicleDestroyed(string guid, string playerName, bool getPilotingMode)
        {
            Guid = guid;
            PlayerName = playerName;
            GetPilotingMode = getPilotingMode;
        }

        public override string ToString()
        {
            return "[VehicleRemove - Vehicle Guid: " + Guid + " PlayerName: " + PlayerName + " GetPilotingMode: " + GetPilotingMode + "]";
        }
    }
}
