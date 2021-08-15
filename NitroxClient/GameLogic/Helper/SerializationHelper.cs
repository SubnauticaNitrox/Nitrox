using System.IO;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class SerializationHelper
    {
        private static readonly FieldInfo identifierIdField = typeof(UniqueIdentifier).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);

        // In the core of protobuf we block hand placed items (such as mushroom and coral) 
        // from being deserialized.  This is because there are so many code paths that spawn
        // them and it would be hard to patch them all.  However, any time we want to do
        // deserialization then we should not be subjected to this behaviour.
        public static bool BLOCK_HAND_PLACED_DESERIALIZATION = true;

        /// See <see cref="UniqueIdentifier_Id_Getter_Patch.Prefix"/> for more info.
        public const string ID_IGNORE_KEY = "SoundsLikeNitrox";

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
            BLOCK_HAND_PLACED_DESERIALIZATION = false;

            GameObject gameObject;

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                gameObject = Serializer.DeserializeObjectTree(memoryStream, 0);
            }

            BLOCK_HAND_PLACED_DESERIALIZATION = true;

            return gameObject;
        }

        /// <summary>
        /// Deserializes the GameObject while ignoring the <see cref="UniqueIdentifier"/> of the parent.
        /// This can be useful to prevent error logging on deserialization because parent (like player inventory) has not the same id anymore.
        /// </summary>
        public static byte[] GetBytesWithoutParent(GameObject gameObject)
        {
            if (!gameObject.transform.parent || !gameObject.transform.parent.TryGetComponent(out UniqueIdentifier parentIdentifier))
            {
                return GetBytes(gameObject);
            }

            string tmpId = parentIdentifier.Id;
            identifierIdField.SetValue(parentIdentifier, ID_IGNORE_KEY);
            byte[] bytes = GetBytes(gameObject);
            identifierIdField.SetValue(parentIdentifier, tmpId);

            return bytes;
        }
    }
}
