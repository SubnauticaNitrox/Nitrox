using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : Packet
    {
        public BasePiece BasePiece;

        public PlaceBasePiece(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - BasePiece: " + BasePiece + "]";
        }
    }
}
