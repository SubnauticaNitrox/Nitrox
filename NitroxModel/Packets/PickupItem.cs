using System;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : Packet
    {
        public NitroxId Id { get; }
        public Vector3 ItemPosition { get; }
        public TechType TechType { get; }

        public PickupItem(Vector3 itemPosition, NitroxId id, TechType techType)
        {
            ItemPosition = itemPosition;
            Id = id;
            TechType = techType;
        }

        // IMPORTANT: At the moment this method will desync players that use certain things (Fabricator or spawn items that contains items)
        // THEREFORE: It is commented out until someone will rework it so the ordered packages will not stop from being processed
        /*public override Optional<AbsoluteEntityCell> GetDeferredCell()
        {
            // Items that are maintained in the global root should never have their pickup event
            // deferred because other players should be able to see their state change.  An example
            // of this is a player picking up a far away beacon.
            if (Map.Main.GlobalRootTechTypes.Contains(TechType))
            {
                return Optional.Empty();
            }

            // All other pickup events should only happen when the cell is loaded.  
            return Optional.Of(new AbsoluteEntityCell(ItemPosition, Map.Main.ItemLevelOfDetail));
        }*/

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + ItemPosition + " Id: " + Id + " TechType: " + TechType + "]";
        }
    }
}
