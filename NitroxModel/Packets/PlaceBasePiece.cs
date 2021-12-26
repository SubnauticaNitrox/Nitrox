using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlaceBasePiece : Packet
    {
        [Index(0)]
        public virtual BasePiece BasePiece { get; protected set; }

        private PlaceBasePiece() { }

        public PlaceBasePiece(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }
    }
}
