using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    /// <summary>
    ///     Used to reference a Unity GameObject and makes it possible to synchronize a GameObject between connected players.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public sealed class NitroxId : ISerializable, IEquatable<NitroxId>, IEqualityComparer<NitroxId>
    {
        [ProtoMember(1)]
        private readonly Guid guid;

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

        public NitroxId(byte[] bytes)
        {
            guid = new Guid(bytes);
        }

        private NitroxId(SerializationInfo info, StreamingContext context)
        {
            byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
            guid = new Guid(bytes);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", guid.ToByteArray());
        }

        public override string ToString()
        {
            return guid.ToString();
        }

        public bool Equals(NitroxId other)
        {
            return guid.Equals(other.guid);
        }

        public bool Equals(NitroxId x, NitroxId y)
        {
            return x is null && y is null || !(x is null) && x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return x is null && y is null || !(x is null) && x.Equals(y);
        }

        public int GetHashCode(NitroxId obj)
        {
            return -1324198676 + obj.guid.GetHashCode();
        }

        public override int GetHashCode()
        {
            return -1324198676 + guid.GetHashCode();
        }
    }
}
