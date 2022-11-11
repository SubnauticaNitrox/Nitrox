using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[ProtoContract]
public class BasicItemData : ItemData
{
    [IgnoreConstructor]
    protected BasicItemData() { }

    public BasicItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData) : base(containerId, itemId, serializedData)
    {
    }
}
