using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Bases
{
    [DataContract]
    public class BaseData
    {
        [DataMember(Order = 1)]
        public List<BasePiece> PartiallyConstructedPieces = new List<BasePiece>();

        [DataMember(Order = 2)]
        public List<BasePiece> CompletedBasePieceHistory = new List<BasePiece>();

        public static BaseData From(List<BasePiece> partiallyConstructedPieces, List<BasePiece> completedBasePieceHistory)
        {
            BaseData baseData = new BaseData();
            baseData.PartiallyConstructedPieces = partiallyConstructedPieces;
            baseData.CompletedBasePieceHistory = completedBasePieceHistory;

            return baseData;
        }
    }
}
