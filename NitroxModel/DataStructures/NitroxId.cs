using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures
{
    /// <summary>
    ///     Used to reference a Unity GameObject and makes it possible to synchronize a GameObject between connected players.
    /// </summary>
    [ProtoContract]
    [ZeroFormattable]
    public class NitroxId : ISerializable
    {
        [ProtoMember(1)]
        [Index(0)]
        public virtual Guid Guid { get; protected set; }

        public NitroxId()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        ///     Create a NitroxId from a string
        /// </summary>
        /// <param name="str">a NitroxID as string</param>
        public NitroxId(string str)
        {
            Guid = new Guid(str);
        }

        public NitroxId(Guid guid)
        {
            Guid = guid;
        }

        public NitroxId(byte[] bytes)
        {
            Guid = new Guid(bytes);
        }

        protected NitroxId(SerializationInfo info, StreamingContext context)
        {
            byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
            Guid = new Guid(bytes);
        }

        public static bool operator ==(NitroxId id1, NitroxId id2)
        {
            if (ReferenceEquals(id1, null))
            {
                if (ReferenceEquals(id2, null))
                {
                    return true;
                }
                return false;
            }
            return id1.Equals(id2);
        }

        public static bool operator !=(NitroxId id1, NitroxId id2)
        {
            return !(id1 == id2);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", Guid.ToByteArray());
        }

        public override bool Equals(object obj)
        {
            NitroxId id = obj as NitroxId;

            return id != null &&
                   Guid.Equals(id.Guid);
        }

        public override int GetHashCode()
        {
            return -1324198676 + EqualityComparer<Guid>.Default.GetHashCode(Guid);
        }

        public override string ToString()
        {
            return Guid.ToString();
        }

        static int[] byteOrder = { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };

        public NitroxId Increment()
        {
            byte[] bytes = Guid.ToByteArray();
            bool canIncrement = byteOrder.Any(i => ++bytes[i] != 0);
            Guid nextGuid = new Guid(canIncrement ? bytes : new byte[16]);

            return new NitroxId(nextGuid);
        }
    }
}
