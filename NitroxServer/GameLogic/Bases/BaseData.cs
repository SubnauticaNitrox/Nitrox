using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Bases;

[JsonObject(MemberSerialization.OptIn)]
public class BaseData
{
    [JsonProperty]
    public List<BasePiece> PartiallyConstructedPieces = new();

    [JsonProperty]
    public List<BasePiece> CompletedBasePieceHistory = new();

    public static BaseData From(List<BasePiece> partiallyConstructedPieces, List<BasePiece> completedBasePieceHistory)
    {
        BaseData baseData = new()
        {
            PartiallyConstructedPieces = partiallyConstructedPieces, 
            CompletedBasePieceHistory = completedBasePieceHistory
        };

        return baseData;
    }
}
