using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Catalog;

public class ContentCatalogData
{
    public Dictionary<object, List<ResourceLocation>> Resources { get; set; }

    private void Read(Json.ContentCatalogData data)
    {
        List<Bucket> buckets;

        MemoryStream bucketStream = new(Convert.FromBase64String(data.BucketDataString));
        using (BinaryReader bucketReader = new(bucketStream))
        {
            int bucketCount = bucketReader.ReadInt32();
            buckets = new List<Bucket>(bucketCount);

            for (int i = 0; i < bucketCount; i++)
            {
                int offset = bucketReader.ReadInt32();

                int entryCount = bucketReader.ReadInt32();
                int[] entries = new int[entryCount];
                for (int j = 0; j < entryCount; j++)
                {
                    entries[j] = bucketReader.ReadInt32();
                }

                buckets.Add(new Bucket(offset, entries));
            }
        }

        List<object> keys;

        MemoryStream keyDataStream = new(Convert.FromBase64String(data.KeyDataString));
        using (BinaryReader keyReader = new(keyDataStream))
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

        MemoryStream entryDataStream = new(Convert.FromBase64String(data.EntryDataString));
        MemoryStream extraDataStream = new(Convert.FromBase64String(data.ExtraDataString));
        using (BinaryReader entryReader = new(entryDataStream))
        using (BinaryReader extraReader = new(extraDataStream))
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

                string internalId = data.InternalIds[internalIdIndex];

                string providerId = data.ProviderIds[providerIndex];

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
                SerializedTypeJson resourceType = data.ResourceTypes[resourceTypeIndex];

                locations.Add(new ResourceLocation(internalId, providerId, dependencyKey, objData, depHash, primaryKey, resourceType));
            }
        }

        Resources = new Dictionary<object, List<ResourceLocation>>(buckets.Count);
        for (int i = 0; i < buckets.Count; i++)
        {
            int[] bucketEntries = buckets[i].entries;
            List<ResourceLocation> locs = new(bucketEntries.Length);
            foreach (int entry in bucketEntries)
            {
                locs.Add(locations[entry]);
            }
            Resources[keys[i]] = locs;
        }
    }

    private struct Bucket
    {
        public int offset;
        public int[] entries;

        public Bucket(int offset, int[] entries)
        {
            this.offset = offset;
            this.entries = entries;
        }
    }
    
    public static ContentCatalogData FromJson(string data)
    {
        Json.ContentCatalogData jsonObj = JsonConvert.DeserializeObject<Json.ContentCatalogData>(data);
        ContentCatalogData ccd = new();
        ccd.Read(jsonObj);
        return ccd;
    }
}
