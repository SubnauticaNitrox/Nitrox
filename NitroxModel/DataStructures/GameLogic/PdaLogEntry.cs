using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[DataContract]
public record PdaLogEntry
{
    [DataMember(Order = 1)]
    public string Key { get; set; }

    [DataMember(Order = 2)]
    public float Timestamp { get; set; }

    [IgnoreConstructor]
    protected PdaLogEntry()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PdaLogEntry(string key, float timestamp)
    {
        Key = key;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return $"{nameof(Key)}: {Key}, {nameof(Timestamp)}: {Timestamp}";
    }
}
