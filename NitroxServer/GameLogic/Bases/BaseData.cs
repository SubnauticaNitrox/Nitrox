using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    public class BaseData
    {
        public const long VERSION = 3;

        [ProtoMember(1)]
        public List<BasePiece> BasePieces = new List<BasePiece>();

        public static BaseData From(List<BasePiece> basePieces)
        {
            BaseData baseData = new BaseData();
            baseData.BasePieces = basePieces;

            return baseData;
        }
    }
}
