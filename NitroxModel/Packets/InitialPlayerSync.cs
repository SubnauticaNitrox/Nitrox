using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public List<BasePiece> BasePieces { get; }

        public InitialPlayerSync(List<BasePiece> basePieces)
        {
            BasePieces = basePieces;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - BasePieces: " + BasePieces + "]";
        }
    }
}
