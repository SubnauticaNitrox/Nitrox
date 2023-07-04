using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper;

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
            CoroutineTask<GameObject> gameObjectTask = Serializer.DeserializeObjectTreeAsync(memoryStream, false, false, 0);
            completeTask(gameObjectTask);
            gameObject = gameObjectTask.GetResult();
        }

        BLOCK_HAND_PLACED_DESERIALIZATION = true;

        return gameObject;
    }

    // Since this class is used synchronously in so many places, we'll just exhaust the enumerator for now to retrieve the result.
    private static void completeTask(IEnumerator gameObjectTask)
    {
        while (gameObjectTask.MoveNext())
        {
            if (gameObjectTask.Current is IEnumerator subroutine)
            {
                completeTask(subroutine);
            }
            else if (gameObjectTask.Current is System.Object)
            {
                return;
            }
        }
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

    public static byte[] CompressBytes(byte[] array)
    {
        using MemoryStream output = new();
        using DeflateStream stream = new(output, System.IO.Compression.CompressionLevel.Optimal);
        CompressStream(stream, array);
        stream.Close();
        return output.ToArray();
    }

    public static byte[] DecompressBytes(byte[] array, int size)
    {
        using MemoryStream input = new(array);
        using DeflateStream stream = new(input, CompressionMode.Decompress);

        return DecompressStream(stream, size);
    }

    public static void CompressStream(Stream stream, byte[] array)
    {
        using BinaryWriter writer = new(stream);

        ushort zeroCounter = 0;
        foreach (byte value in array)
        {
            if (value == 0)
            {
                zeroCounter++;
            }
            else
            {
                writer.Write(zeroCounter);
                zeroCounter = 0;

                writer.Write(value);
            }
        }

        if (zeroCounter != 0)
        {
            writer.Write(zeroCounter);
        }

        writer.Close();
    }

    public static byte[] DecompressStream(Stream stream, int size)
    {
        using BinaryReader reader = new(stream);
        byte[] result = new byte[size];

        int i = 0;
        bool zeroPart = true;
        while (i < size)
        {
            if (zeroPart)
            {
                ushort zeroLength = reader.ReadUInt16();

                for (int c = 0; c < zeroLength; c++)
                {
                    result[i] = 0;
                    i++;
                }
            }
            else
            {
                result[i] = reader.ReadByte();
                i++;
            }

            zeroPart = !zeroPart;
        }

        return result;
    }
}
