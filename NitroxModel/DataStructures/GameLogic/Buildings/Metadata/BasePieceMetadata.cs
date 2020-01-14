using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(SignMetadata))]
    public abstract class BasePieceMetadata
    {
    }
}
