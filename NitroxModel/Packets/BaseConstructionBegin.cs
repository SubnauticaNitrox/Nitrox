using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseConstructionBegin : Packet
    {
        public BasePiece BasePiece;

        public BaseConstructionBegin(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - BasePiece: " + BasePiece + "]";
        }
    }
}
