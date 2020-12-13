using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;

namespace Nitrox.Model.Packets
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
