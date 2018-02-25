using System;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class DeferrablePacket : Packet
    {
        public const int BUILDING_CELL_LEVEL = 3;
        public const int ITEM_INTERACTION_CELL_LEVEL = 3;

        public AbsoluteEntityCell AbsoluteEntityCell { get; }

        public DeferrablePacket(AbsoluteEntityCell absoluteEntityCell)
        {
            AbsoluteEntityCell = absoluteEntityCell;
        }

        public DeferrablePacket(Vector3 worldSpace, int level) : this(new AbsoluteEntityCell(worldSpace, level))
        {
        }
    }
}
