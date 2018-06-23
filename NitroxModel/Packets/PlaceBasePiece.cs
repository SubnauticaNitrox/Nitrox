using System;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : DeferrablePacket
    {
        public BasePiece BasePiece;

        public PlaceBasePiece(BasePiece basePiece, Vector3 placedPosition) : base(placedPosition, BUILDING_CELL_LEVEL)
        {
            BasePiece = basePiece;
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - BasePiece: " + BasePiece + "]";
        }
    }
}
