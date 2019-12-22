using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [ProtoContract]
    [Serializable]
    public class NitroxId : ISerializable
    {
        [ProtoMember(1)]
        private Guid guid;

        public static NitroxId Empty { get; } = new NitroxId(Guid.Empty);

        public NitroxId()
        {
            guid = Guid.NewGuid();
        }
        
        public NitroxId(byte[] bytes)
        {
            guid = new Guid(bytes);
        }

        public NitroxId(Guid guid)
        {
            this.guid = guid;
        }

        public static NitroxId Parse(string input)
        { 
            return new NitroxId(Guid.Parse(input));
        }

        protected NitroxId(SerializationInfo info, StreamingContext context)
        {
            byte[] bytes = (byte[])info.GetValue("id", typeof(byte[]));
            guid = new Guid(bytes);
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

        public static bool operator ==(NitroxId id1, NitroxId id2)
        {
            return id1?.guid == id2?.guid;
        }

        public static bool operator !=(NitroxId id1, NitroxId id2)
        {
            return !(id1 == id2);
        }
    }
}
