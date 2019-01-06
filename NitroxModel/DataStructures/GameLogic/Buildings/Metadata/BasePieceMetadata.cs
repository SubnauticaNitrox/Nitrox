using System;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(SignMetadata))]
    public abstract class BasePieceMetadata
    {
    }
}
