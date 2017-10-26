using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : AuthenticatedPacket
    {
        public String Guid { get; }

        public SimulationOwnershipRequest(String playerId, String guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
