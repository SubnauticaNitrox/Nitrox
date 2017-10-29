using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : AuthenticatedPacket
    {
        public string Guid { get; }

        public SimulationOwnershipRequest(string playerId, string guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
