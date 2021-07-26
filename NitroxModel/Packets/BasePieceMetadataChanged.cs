using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BasePieceMetadataChanged : Packet
    {
        public NitroxId PieceId { get; }
        public BasePieceMetadata Metadata { get; }

        public BasePieceMetadataChanged(NitroxId pieceId, BasePieceMetadata metadata)
        {
            PieceId = pieceId;
            Metadata = metadata;
        }
    }
}
