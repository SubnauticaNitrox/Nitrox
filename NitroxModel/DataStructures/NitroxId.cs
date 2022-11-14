using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures;

/// <summary>
///     Used to reference a Unity GameObject and makes it possible to synchronize a GameObject between connected players.
/// </summary>
[ProtoContract]
[Serializable]
public class NitroxId : ISerializable, IEquatable<NitroxId>
{
    [ProtoMember(1)]
    [SerializableMember]
    private Guid guid { get; init; }

    [IgnoreConstructor]
    public NitroxId()
    {
        guid = Guid.NewGuid();
    }

    /// <summary>
    ///     Create a NitroxId from a string
    /// </summary>
    /// <param name="str">a NitroxID as string</param>
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

    protected NitroxId(SerializationInfo info, StreamingContext context)
    {
        byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
        guid = new Guid(bytes);
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("id", guid.ToByteArray());
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        
        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((NitroxId)obj);
    }

        
    public bool Equals(NitroxId other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return guid.Equals(other.guid);
    }
        
    public override int GetHashCode()
    {
        return guid.GetHashCode();
    }

    public override string ToString()
    {
        return guid.ToString();
    }

    [IgnoredMember]
    private static int[] byteOrder = { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };

    public NitroxId Increment()
    {
        byte[] bytes = guid.ToByteArray();
        bool canIncrement = byteOrder.Any(i => ++bytes[i] != 0);
        Guid nextGuid = new(canIncrement ? bytes : new byte[16]);

        return new NitroxId(nextGuid);
    }

}
