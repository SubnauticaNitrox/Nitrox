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
    public class NitroxId : ISerializable
    {
        [ProtoMember(1)]
        private Guid guid;

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

        protected NitroxId(SerializationInfo info, StreamingContext context)
        {
            byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
            guid = new Guid(bytes);
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
            info.AddValue("id", guid.ToByteArray());
        }

        public override bool Equals(object obj)
        {
            NitroxId id = obj as NitroxId;

            return id != null &&
                   guid.Equals(id.guid);
        }

        public override int GetHashCode()
        {
            return -1324198676 + EqualityComparer<Guid>.Default.GetHashCode(guid);
        }

        public override string ToString()
        {
            return guid.ToString();
        }
    }
}
