using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BasePieceMetadataChanged : Packet
    {
        public NitroxId BaseParentId { get; }
        public NitroxId PieceId { get; }
        public BasePieceMetadata Metadata { get; }

        public BasePieceMetadataChanged(NitroxId baseParentId, NitroxId pieceId, BasePieceMetadata metadata)
        {
            BaseParentId = baseParentId;
            PieceId = pieceId;
            Metadata = metadata;
        }
    }
}
