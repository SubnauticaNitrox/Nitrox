using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(KeypadMetadata))]
    public abstract class EntityMetadata
    {
    }
}
