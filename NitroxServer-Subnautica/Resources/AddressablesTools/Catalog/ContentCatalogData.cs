using AddressablesTools.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AddressablesTools.Catalog
{
    [Newtonsoft.Json.JsonObject(MemberSerialization.Fields)]
    public class ContentCatalogData
    {
        public string LocatorId { get; set; }
        public ObjectInitializationData InstanceProviderData { get; set; }
        public ObjectInitializationData SceneProviderData { get; set; }
        public ObjectInitializationData[] ResourceProviderData { get; set; }
        // used for resources, shouldn't be edited directly
        protected string[] ProviderIds { get; set; }
        protected string[] InternalIds { get; set; }
        protected SerializedType[] ResourceTypes { get; set; }
        private string[] InternalIdPrefixes { get; set; } // todo

        public Dictionary<object, List<ResourceLocation>> Resources { get; set; }

        internal void Read(ContentCatalogDataJson data)
        {
            LocatorId = data.m_LocatorId;

            InstanceProviderData = new ObjectInitializationData();
            InstanceProviderData.Read(data.m_InstanceProviderData);

            SceneProviderData = new ObjectInitializationData();
            SceneProviderData.Read(data.m_SceneProviderData);

            ResourceProviderData = new ObjectInitializationData[data.m_ResourceProviderData.Length];
            for (int i = 0; i < ResourceProviderData.Length; i++)
            {
                ResourceProviderData[i] = new ObjectInitializationData();
                ResourceProviderData[i].Read(data.m_ResourceProviderData[i]);
            }

            ProviderIds = new string[data.m_ProviderIds.Length];
            for (int i = 0; i < ProviderIds.Length; i++)
            {
                ProviderIds[i] = data.m_ProviderIds[i];
            }

            InternalIds = new string[data.m_InternalIds.Length];
            for (int i = 0; i < InternalIds.Length; i++)
            {
                InternalIds[i] = data.m_InternalIds[i];
            }

            ResourceTypes = new SerializedType[data.m_resourceTypes.Length];
            for (int i = 0; i < ResourceTypes.Length; i++)
            {
                ResourceTypes[i] = new SerializedType();
                ResourceTypes[i].Read(data.m_resourceTypes[i]);
            }

            InternalIdPrefixes = new string[data.m_InternalIdPrefixes.Length];
            for (int i = 0; i < InternalIdPrefixes.Length; i++)
            {
                InternalIdPrefixes[i] = data.m_InternalIdPrefixes[i];
            }

            ReadResources(data);
        }

        private void ReadResources(ContentCatalogDataJson data)
        {
            List<Bucket> buckets;

            MemoryStream bucketStream = new MemoryStream(Convert.FromBase64String(data.m_BucketDataString));
            using (BinaryReader bucketReader = new BinaryReader(bucketStream))
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

            MemoryStream keyDataStream = new MemoryStream(Convert.FromBase64String(data.m_KeyDataString));
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

            MemoryStream entryDataStream = new MemoryStream(Convert.FromBase64String(data.m_EntryDataString));
            MemoryStream extraDataStream = new MemoryStream(Convert.FromBase64String(data.m_ExtraDataString));
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

            Resources = new Dictionary<object, List<ResourceLocation>>(buckets.Count);
            for (int i = 0; i < buckets.Count; i++)
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

        internal void Write(ContentCatalogDataJson data)
        {
            data.m_LocatorId = LocatorId;

            data.m_InstanceProviderData = new ObjectInitializationDataJson();
            InstanceProviderData.Write(data.m_InstanceProviderData);

            data.m_SceneProviderData = new ObjectInitializationDataJson();
            SceneProviderData.Write(data.m_SceneProviderData);

            data.m_ResourceProviderData = new ObjectInitializationDataJson[ResourceProviderData.Length];
            for (int i = 0; i < data.m_ResourceProviderData.Length; i++)
            {
                data.m_ResourceProviderData[i] = new ObjectInitializationDataJson();
                ResourceProviderData[i].Write(data.m_ResourceProviderData[i]);
            }

            WriteResources(data);

            data.m_ProviderIds = new string[ProviderIds.Length];
            for (int i = 0; i < data.m_ProviderIds.Length; i++)
            {
                data.m_ProviderIds[i] = ProviderIds[i];
            }

            data.m_InternalIds = new string[InternalIds.Length];
            for (int i = 0; i < data.m_InternalIds.Length; i++)
            {
                data.m_InternalIds[i] = InternalIds[i];
            }

            data.m_resourceTypes = new SerializedTypeJson[ResourceTypes.Length];
            for (int i = 0; i < data.m_resourceTypes.Length; i++)
            {
                data.m_resourceTypes[i] = new SerializedTypeJson();
                ResourceTypes[i].Write(data.m_resourceTypes[i]);
            }

            data.m_InternalIdPrefixes = new string[InternalIdPrefixes.Length];
            for (int i = 0; i < data.m_InternalIdPrefixes.Length; i++)
            {
                data.m_InternalIdPrefixes[i] = InternalIdPrefixes[i];
            }
        }

        private void WriteResources(ContentCatalogDataJson data)
        {
            HashSet<string> newInternalIdHs = new HashSet<string>();
            HashSet<string> newProviderIdHs = new HashSet<string>();
            HashSet<SerializedType> newResourceTypeHs = new HashSet<SerializedType>();
            //HashSet<string> newInternalIdPrefixes = new HashSet<string>(); // todo

            HashSet<ResourceLocation> newLocationHs = new HashSet<ResourceLocation>();

            List<object> newKeys = Resources.Keys.ToList();

            foreach (var value in Resources.Values)
            {
                foreach (var location in value)
                {
                    newLocationHs.Add(location);

                    if (location.InternalId == null)
                        throw new Exception("Location's internal ID cannot be null!");

                    if (location.ProviderId == null)
                        throw new Exception("Location's provider ID cannot be null!");

                    newInternalIdHs.Add(location.InternalId);
                    newProviderIdHs.Add(location.ProviderId);

                    if (location.Type != null)
                    {
                        newResourceTypeHs.Add(location.Type);
                    }
                }
            }

            List<string> newInternalIds = newInternalIdHs.ToList();
            List<string> newProviderIds = newProviderIdHs.ToList();
            List<SerializedType> newResourceTypes = newResourceTypeHs.ToList();
            List<ResourceLocation> newLocations = newLocationHs.ToList();

            MemoryStream entryDataStream = new MemoryStream();
            MemoryStream extraDataStream = new MemoryStream();
            using (BinaryWriter entryWriter = new BinaryWriter(entryDataStream))
            using (BinaryWriter extraWriter = new BinaryWriter(extraDataStream))
            {
                entryWriter.Write(newLocationHs.Count);

                foreach (var location in newLocationHs)
                {
                    int internalIdIndex = newInternalIds.IndexOf(location.InternalId);
                    int providerIndex = newProviderIds.IndexOf(location.ProviderId);
                    int dependencyKeyIndex = (location.Dependency == null) ? -1 : newKeys.IndexOf(location.Dependency);
                    int depHash = location.DependencyHashCode; // todo calculate this
                    int dataIndex = -1;
                    if (location.Data != null)
                    {
                        dataIndex = (int)extraDataStream.Position;
                        SerializedObjectDecoder.Encode(extraWriter, location.Data);
                    }
                    int primaryKeyIndex = newKeys.IndexOf(location.PrimaryKey);
                    int resourceTypeIndex = newResourceTypes.IndexOf(location.Type);

                    entryWriter.Write(internalIdIndex);
                    entryWriter.Write(providerIndex);
                    entryWriter.Write(dependencyKeyIndex);
                    entryWriter.Write(depHash);
                    entryWriter.Write(dataIndex);
                    entryWriter.Write(primaryKeyIndex);
                    entryWriter.Write(resourceTypeIndex);
                }
            }

            MemoryStream keyDataStream = new MemoryStream();
            MemoryStream bucketStream = new MemoryStream();
            using (BinaryWriter keyWriter = new BinaryWriter(keyDataStream))
            using (BinaryWriter bucketWriter = new BinaryWriter(bucketStream))
            {
                keyWriter.Write(newKeys.Count); // same as Resources.Count
                bucketWriter.Write(newKeys.Count);

                foreach (var resourceKvp in Resources)
                {
                    object resourceKey = resourceKvp.Key;
                    List<ResourceLocation> resourceValue = resourceKvp.Value;

                    Bucket bucket = new Bucket
                    {
                        offset = (int)keyDataStream.Position,
                        entries = new int[resourceValue.Count]
                    };

                    // write key
                    SerializedObjectDecoder.Encode(keyWriter, resourceKey);

                    for (int i = 0; i < resourceValue.Count; i++)
                    {
                        bucket.entries[i] = newLocations.IndexOf(resourceValue[i]);
                    }

                    // write bucket
                    bucketWriter.Write(bucket.offset);
                    bucketWriter.Write(bucket.entries.Length);
                    for (int i = 0; i < bucket.entries.Length; i++)
                    {
                        bucketWriter.Write(bucket.entries[i]);
                    }
                }
            }

            ProviderIds = newProviderIds.ToArray();
            InternalIds = newInternalIds.ToArray();
            ResourceTypes = newResourceTypes.ToArray();

            data.m_BucketDataString = Convert.ToBase64String(bucketStream.ToArray());
            data.m_KeyDataString = Convert.ToBase64String(keyDataStream.ToArray());
            data.m_EntryDataString = Convert.ToBase64String(entryDataStream.ToArray());
            data.m_ExtraDataString = Convert.ToBase64String(extraDataStream.ToArray());
        }

        protected struct Bucket
        {
            public int offset;
            public int[] entries;

            public Bucket(int offset, int[] entries)
            {
                this.offset = offset;
                this.entries = entries;
            }
            public static Bucket ReadBinary(BinaryReader reader)
            {
                int offset = reader.ReadInt32();

                int arrayLength = reader.ReadInt32();
                int[] entries = new int[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    entries[i] = reader.ReadInt32();
                }

                return new Bucket(offset, entries);
            }
        }
    }
}
