using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [ProtoContract]
    [Serializable]
    public class NitroxId : ISerializable
    {
        private static readonly Guid nitroxNamespaceId = new Guid("7eaf7622-f2b4-43f3-86a5-f818616911f8");

        [ProtoMember(1)]
        private Guid guid;

        public NitroxId()
        {
            guid = Guid.NewGuid();
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

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", guid.ToByteArray());
        }

        /// <summary>
        /// Creates a deterministic NitroxId from a given input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A deterministic NitroxId derived from the input.</returns>
        public static NitroxId From(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Create a name-based GUID using the algorithm from RFC 4122 §4.3

            // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
            byte[] nameBytes = Encoding.UTF8.GetBytes(input);

            // convert the namespace GUID to network order (step 3)
            byte[] namespaceBytes = nitroxNamespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // compute the hash of the namespace ID concatenated with the name (step 4)
            byte[] hash;
            using (SHA1 shaAlgorithm = SHA1.Create())
            {
                shaAlgorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                shaAlgorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = shaAlgorithm.Hash;
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            byte[] newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte)((newGuid[6] & 0x0F) | 0x80);

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting GUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new NitroxId(newGuid);
        }

        // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
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
