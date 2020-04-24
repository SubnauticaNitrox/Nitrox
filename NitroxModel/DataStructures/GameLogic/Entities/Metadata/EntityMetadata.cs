using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(KeypadMetadata)), ProtoInclude(60, typeof(SealedDoorMetadata))]    
    public abstract class EntityMetadata
    {
    }
}
