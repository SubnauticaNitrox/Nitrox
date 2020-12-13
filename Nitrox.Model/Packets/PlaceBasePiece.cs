using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
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
