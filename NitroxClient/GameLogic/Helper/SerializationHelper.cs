using System.IO;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class SerializationHelper
    {
        private static ProtobufSerializer Serializer => ProtobufSerializerPool.GetProxy().Value;

        public static byte[] GetBytes(GameObject gameObject)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Serializer.SerializeObjectTree(memoryStream, gameObject);
                return memoryStream.ToArray();
            }
        }

        public static GameObject GetGameObject(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                return Serializer.DeserializeObjectTree(memoryStream, 0);
            }
        }
    }
}
