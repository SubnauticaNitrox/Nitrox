using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : Packet
    {
        public BasePiece BasePiece { get; }

        public PlaceBasePiece(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }
    }
}
