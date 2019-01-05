using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : Packet
    {
        public string Guid { get; }
        public Vector3 ItemPosition { get; }
        public TechType TechType { get; }

        public PickupItem(Vector3 itemPosition, string guid, TechType techType)
        {
            ItemPosition = itemPosition;
            Guid = guid;
            TechType = techType;
        }

        public override Optional<AbsoluteEntityCell> GetDeferredCell()
        {
            // Items that are maintained in the global root should never have their pickup event
            // deferred because other players should be able to see their state change.  An example
            // of this is a player picking up a far away beacon.
            if (Map.GLOBAL_ROOT_TECH_TYPES.Contains(TechType))
            {
                return Optional<AbsoluteEntityCell>.Empty();
            }

            // All other pickup events should only happen when the cell is loaded.  
            return Optional<AbsoluteEntityCell>.Of(new AbsoluteEntityCell(ItemPosition, Map.ITEM_LEVEL_OF_DETAIL));
        }

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + ItemPosition + " Guid: " + Guid + " TechType: " + TechType + "]";
        }
    }
}
