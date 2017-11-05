using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<OwnedGuid> OwnedGuids { get; }

        public SimulationOwnershipChange(string guid, string owningPlayerId)
        {
            OwnedGuids = new List<OwnedGuid>
            {
                new OwnedGuid(guid, owningPlayerId, false)
            };
        }

        public SimulationOwnershipChange(List<OwnedGuid> ownedGuids)
        {
            OwnedGuids = ownedGuids;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("[SimulationOwnershipChange - ");

            foreach (OwnedGuid ownedGuid in OwnedGuids)
            {
                stringBuilder.Append(ownedGuid.ToString());
            }

            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}
