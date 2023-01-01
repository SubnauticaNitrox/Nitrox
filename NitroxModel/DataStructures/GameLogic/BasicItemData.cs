using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[DataContract]
public class BasicItemData : ItemData
{
    [IgnoreConstructor]
    protected BasicItemData() { }

    public BasicItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData) : base(containerId, itemId, serializedData)
    {
    }
}
