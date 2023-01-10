using System;
using System.Runtime.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(SignMetadata))]
    public abstract class BasePieceMetadata
    {
    }
}
