using System.IO;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public static class SerializationHelper
    {
        // In the core of protobuf we block hand placed items (such as mushroom and coral) 
        // from being deserialized.  This is because there are so many code paths that spawn
        // them and it would be hard to patch them all.  However, any time we want to do
        // deserialization then we should not be subjected to this behaviour.
        private static bool blockHandPlacedDeserialization = true;

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
            blockHandPlacedDeserialization = false;

            GameObject gameObject;

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                gameObject = Serializer.DeserializeObjectTree(memoryStream, 0);
            }

            blockHandPlacedDeserialization = true;

            return gameObject;
        }
    }
}
