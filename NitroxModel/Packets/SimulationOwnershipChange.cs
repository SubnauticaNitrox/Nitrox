using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<OwnedGuid> OwnedGuids { get; }
        
        public SimulationOwnershipChange(String guid, String owningPlayerId) : base()
        {
            this.OwnedGuids = new List<OwnedGuid>();
            this.OwnedGuids.Add(new OwnedGuid(guid, owningPlayerId));
        }

        public SimulationOwnershipChange(List<OwnedGuid> ownedGuids) : base()
        {
            this.OwnedGuids = ownedGuids;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("[SimulationOwnershipChange - ");

            foreach(OwnedGuid ownedGuid in OwnedGuids)
            {
                stringBuilder.Append(ownedGuid.ToString());
            }
            
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}
