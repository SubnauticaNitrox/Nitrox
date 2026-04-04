using System;
using System.Linq;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.DataStructures;

/// <summary>
///     Used to reference a Unity GameObject and makes it possible to synchronize a GameObject between connected players.
/// </summary>
[Serializable]
[DataContract]
public sealed class NitroxId : ISerializable, IEquatable<NitroxId>, IComparable<NitroxId>
{
    [IgnoredMember]
    private static readonly int[] byteOrder = [15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3];


    [DataMember(Order = 1)]
    [SerializableMember]
    private Guid guid { get; init; }

    [IgnoreConstructor]
    public NitroxId()
    {
        guid = Guid.NewGuid();
    }

    public NitroxId(string str)
    {
        guid = new Guid(str);
    }

    public NitroxId(Guid guid)
    {
        this.guid = guid;
    }

    public NitroxId(byte[] bytes)
    {
        guid = new Guid(bytes);
    }

    private NitroxId(SerializationInfo info, StreamingContext context)
    {
        byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
        guid = new Guid(bytes);
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("id", guid.ToByteArray());
    }

    public static bool operator ==(NitroxId? id1, NitroxId? id2)
    {
        if (id1 is not null)
        {
            return id1.Equals(id2);
        }

        return id2 is null;
    }

    public static bool operator !=(NitroxId? id1, NitroxId? id2)
    {
        return !(id1 == id2);
    }

    public static implicit operator NitroxId(string str)
    {
        return new NitroxId(str);
    }

    public static implicit operator NitroxId(Guid guid)
    {
        return new NitroxId(guid);
    }

    public override bool Equals(object? obj) => Equals(obj as NitroxId);

    public bool Equals(NitroxId? other)
    {
        return other is not null && guid.Equals(other.guid);
    }

    public override int GetHashCode() => guid.GetHashCode();

    public override string ToString() => guid.ToString();

    public NitroxId Increment()
    {
        byte[] bytes = guid.ToByteArray();
        bool canIncrement = byteOrder.Any(i => ++bytes[i] != 0);
        Guid nextGuid = new(canIncrement ? bytes : new byte[16]);

        return new NitroxId(nextGuid);
    }

    public int CompareTo(NitroxId? other)
    {
        if (Equals(this, other))
        {
            return 0;
        }

        return other is null ? 1 : guid.CompareTo(other.guid);
    }
}
