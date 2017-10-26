using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<OwnedGuid> OwnedGuids { get; } = new List<OwnedGuid>();

        public SimulationOwnershipChange(String guid, String owningPlayerId) : base()
        {
            OwnedGuids.Add(new OwnedGuid(guid, owningPlayerId));
        }

        public SimulationOwnershipChange(List<OwnedGuid> ownedGuids) : base()
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
