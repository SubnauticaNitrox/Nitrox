using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRelease : Packet
    {
        public string PlayerId { get; }
        public string Guid { get; }

        public SimulationOwnershipRelease(string playerId, string guid)
        {
            PlayerId = playerId;
            Guid = guid;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRelease - Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
