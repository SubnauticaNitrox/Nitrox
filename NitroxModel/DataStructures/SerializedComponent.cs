using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures;

/// <summary>
/// Holds an Unity component's data to be restored on clients.
/// </summary>
[Serializable, DataContract]
public class SerializedComponent
{
    [DataMember(Order = 1)]
    public string TypeName { get; set; }

    [DataMember(Order = 2)]
    public bool IsEnabled { get; set; }

    [DataMember(Order = 3)]
    public byte[] Data { get; set; }

    protected SerializedComponent()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SerializedComponent(string typeName, bool isEnabled, byte[] data)
    {
        TypeName = typeName;
        IsEnabled = isEnabled;
        Data = data;
    }
}
