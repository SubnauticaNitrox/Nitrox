using System.Collections;
using System.IO;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper;

public static class SerializationHelper
{
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
            CoroutineTask<GameObject> gameObjectTask = Serializer.DeserializeObjectTreeAsync(memoryStream, false, false, 0);
            CompleteTask(gameObjectTask);
            gameObject = gameObjectTask.GetResult();
        }

        BLOCK_HAND_PLACED_DESERIALIZATION = true;

        return gameObject;
    }

    // Since this class is used synchronously in so many places, we'll just exhaust the enumerator for now to retrieve the result.
    private static void CompleteTask(IEnumerator gameObjectTask)
    {
        while (gameObjectTask.MoveNext())
        {
            if (gameObjectTask.Current is IEnumerator subroutine)
            {
                CompleteTask(subroutine);
            }
            else if (gameObjectTask.Current is System.Object)
            {
                return;
            }
        }
    }
}
