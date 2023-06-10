using AddressablesTools.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using AddressablesTools.Catalog;

namespace AddressablesTools.Binary
{
    public class ContentCatalogDataBinary : ContentCatalogData
    {
        private byte[] keyData;
        private Bucket[] buckets;
        private byte[] entryData;
        private byte[] extraData;

        public void ReadBinary(BinaryReader reader)
        {
            LocatorId = reader.ReadString();
            InstanceProviderData = ObjectInitializationDataBinary.ReadBinary(reader);
            SceneProviderData = ObjectInitializationDataBinary.ReadBinary(reader);

            ResourceProviderData = ReadObjectArray<ObjectInitializationData>(reader, ObjectInitializationDataBinary.ReadBinary);

            ProviderIds = ReadStringArray(reader);
            InternalIds = ReadStringArray(reader);
            keyData = ReadByteArray(reader);

            buckets = ReadObjectArray(reader, Bucket.ReadBinary);

            entryData = ReadByteArray(reader);
            extraData = ReadByteArray(reader);

            ResourceTypes = ReadObjectArray<SerializedType>(reader, SerializedTypeBinary.ReadBinary);

            ReadResources();

        }

        private void ReadResources()
        {

            List<object> keys;

            MemoryStream keyDataStream = new MemoryStream(keyData);
            using (BinaryReader keyReader = new BinaryReader(keyDataStream))
            {
                int keyCount = keyReader.ReadInt32();
                keys = new List<object>(keyCount);

                for (int i = 0; i < keyCount; i++)
                {
                    keyDataStream.Position = buckets[i].offset;
                    keys.Add(SerializedObjectDecoder.Decode(keyReader));
                }
            }

            List<ResourceLocation> locations;

            MemoryStream entryDataStream = new MemoryStream(entryData);
            MemoryStream extraDataStream = new MemoryStream(extraData);
            using (BinaryReader entryReader = new BinaryReader(entryDataStream))
            using (BinaryReader extraReader = new BinaryReader(extraDataStream))
            {
                int entryCount = entryReader.ReadInt32();
                locations = new List<ResourceLocation>(entryCount);

                for (int i = 0; i < entryCount; i++)
                {
                    int internalIdIndex = entryReader.ReadInt32();
                    int providerIndex = entryReader.ReadInt32();
                    int dependencyKeyIndex = entryReader.ReadInt32();
                    int depHash = entryReader.ReadInt32();
                    int dataIndex = entryReader.ReadInt32();
                    int primaryKeyIndex = entryReader.ReadInt32();
                    int resourceTypeIndex = entryReader.ReadInt32();

                    string internalId = InternalIds[internalIdIndex];

                    string providerId = ProviderIds[providerIndex];

                    object dependencyKey = null;
                    if (dependencyKeyIndex >= 0)
                    {
                        dependencyKey = keys[dependencyKeyIndex];
                    }

                    object objData = null;
                    if (dataIndex >= 0)
                    {
                        extraDataStream.Position = dataIndex;
                        objData = SerializedObjectDecoder.Decode(extraReader);
                    }

                    object primaryKey = keys[primaryKeyIndex];
                    SerializedType resourceType = ResourceTypes[resourceTypeIndex];

                    var loc = new ResourceLocation();
                    loc.ReadCompact(internalId, providerId, dependencyKey, objData, depHash, primaryKey, resourceType);
                    locations.Add(loc);
                }
            }

            Resources = new Dictionary<object, List<ResourceLocation>>(buckets.Length);
            for (int i = 0; i < buckets.Length; i++)
            {
                int[] bucketEntries = buckets[i].entries;
                List<ResourceLocation> locs = new List<ResourceLocation>(bucketEntries.Length);
                for (int j = 0; j < bucketEntries.Length; j++)
                {
                    locs.Add(locations[bucketEntries[j]]);
                }
                Resources[keys[i]] = locs;
            }
        }

        private static T[] ReadObjectArray<T>(BinaryReader reader, Func<BinaryReader, T> deserializer)
        {
            int length = reader.ReadInt32();
            T[] arr = new T[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = deserializer(reader);
            }

            return arr;
        }

        private static string[] ReadStringArray(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            string[] arr = new string[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = reader.ReadString();
            }

            return arr;
        }

        private static byte[] ReadByteArray(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            byte[] arr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = reader.ReadByte();
            }

            return arr;
        }
    }
}
