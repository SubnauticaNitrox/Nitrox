using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class BasePieceMetadataChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId PieceId { get; protected set; }
        [Index(1)]
        public virtual BasePieceMetadata Metadata { get; protected set; }

        public BasePieceMetadataChanged() { }

        public BasePieceMetadataChanged(NitroxId pieceId, BasePieceMetadata metadata)
        {
            PieceId = pieceId;
            Metadata = metadata;
        }
    }
}
