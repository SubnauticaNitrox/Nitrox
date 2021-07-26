using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    public class BaseData
    {
        public const long VERSION = 2;
        
        [ProtoMember(1)]
        public List<BasePiece> PartiallyConstructedPieces = new List<BasePiece>();

        [ProtoMember(2)]
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
