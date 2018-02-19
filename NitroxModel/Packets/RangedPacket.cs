using System;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class RangedPacket : Packet
    {
        public AbsoluteEntityCell AbsoluteEntityCell { get; }

        public RangedPacket(AbsoluteEntityCell absoluteEntityCell)
        {
            AbsoluteEntityCell = absoluteEntityCell;
        }

        public RangedPacket(Vector3 worldSpace, int level = 1) : this(new AbsoluteEntityCell(worldSpace, level))
        {
        }
    }
}
