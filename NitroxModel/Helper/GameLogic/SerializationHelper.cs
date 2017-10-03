using System.IO;
using UnityEngine;

namespace NitroxModel.Helper.GameLogic
{
    public class SerializationHelper
    {
        private static ProtobufSerializer serializer;

        public static byte[] GetBytes(GameObject gameObject)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                GetSerializer().SerializeObjectTree(memoryStream, gameObject);
                return memoryStream.ToArray();
            }
        }

        public static GameObject GetGameObject(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                return GetSerializer().DeserializeObjectTree(memoryStream, 0);
            }
        }

        private static ProtobufSerializer GetSerializer() // Not directly intialized because it can cause issues in the client tester
        {
            if(serializer == null)
            {
                serializer = new ProtobufSerializer();
            }

            return serializer;
        }
    }
}
