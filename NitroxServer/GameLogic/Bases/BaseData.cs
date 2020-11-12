﻿using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class BaseData
    {
        public const short VERSION = 2;

        [JsonProperty, ProtoMember(1)]
        public List<BasePiece> PartiallyConstructedPieces = new List<BasePiece>();

        [JsonProperty, ProtoMember(2)]
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
