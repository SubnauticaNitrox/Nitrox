using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : Packet
    {
        public string PlayerId { get; }
        public string Guid { get; }

        public SimulationOwnershipRequest(string playerId, string guid)
        {
            PlayerId = playerId;
            Guid = guid;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - PlayerId: " + PlayerId + " Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
