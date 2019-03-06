using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxServer.UnityStubs;
using NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers.Abstract;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers
{
    public class DefaultMonoBehaviourParser : MonoBehaviourParser
    {
        HashSet<long> checkedGameObjects = new HashSet<long>();

        public override void Parse(AssetsFileInstance instance, long gameObjectPathId, ResourceAssets resourceAssets)
        {
            AssetsFile resourcesFile = instance.file;
            AssetsFileReader afr = resourcesFile.reader;
            AssetsFileTable resourcesFileTable = instance.table;

            if (gameObjectPathId == 0)
            {
                return; // Gotta love early breaks
            }
            else if (!checkedGameObjects.Contains(gameObjectPathId))
            {
                checkedGameObjects.Add(gameObjectPathId);
                AssetFileInfoEx asset = ResourceAssetsHelper.FindComponentOfType<PrefabPlaceholdersGroup>(instance, (ulong)gameObjectPathId);

                if (asset != null)
                {
                    ulong prevPos = resourcesFile.reader.Position;
                    resourcesFile.reader.Position = asset.absoluteFilePos + 32;

                    int placeholders = resourcesFile.reader.ReadInt32(); //Array size
                    List<UwePrefab> prefabs = new List<UwePrefab>();
                    for (int a = 0; a < placeholders; a++)
                    {
                        resourcesFile.reader.ReadInt32();//FileId
                        ulong pathId = (ulong)resourcesFile.reader.ReadInt64();//PathId
                        AssetFileInfoEx prefabPlaceholder = resourcesFileTable.getAssetInfo(pathId);
                        ulong placeholderPos = resourcesFile.reader.Position;
                        resourcesFile.reader.Position = prefabPlaceholder.absoluteFilePos;
                        resourcesFile.reader.ReadInt32();
                        ulong gameObjectPath = (ulong)resourcesFile.reader.ReadInt64();
                        AssetFileInfoEx transformAsset = ResourceAssetsHelper.FindTransform(instance, gameObjectPath);

                        ulong curPos = resourcesFile.reader.Position;
                        resourcesFile.reader.Position = transformAsset.absoluteFilePos + 12;

                        Quaternion rotation = new Quaternion(
                            afr.ReadSingle(), // Quaternion X
                            afr.ReadSingle(), // Quaternion Y
                            afr.ReadSingle(), // Quaternion Z
                            afr.ReadSingle()); // Quaternion W

                        Vector3 position = new Vector3(
                            afr.ReadSingle(), // Position X
                            afr.ReadSingle(), // Position Y
                            afr.ReadSingle()); // Position Z

                        Vector3 scale = new Vector3(
                            afr.ReadSingle(), // Scale X
                            afr.ReadSingle(), // Scale Y
                            afr.ReadSingle()); // Scale Z

                        resourcesFile.reader.Position = curPos;
                        resourcesFile.reader.Position += 16;

                        resourcesFile.reader.ReadCountStringInt32();//Empty...
                        string prefabString = resourcesFile.reader.ReadCountStringInt32();

                        prefabs.Add(new UwePrefab(prefabString, position, rotation, scale));
                        resourcesFile.reader.Position = placeholderPos; // Reset position
                    }

                    AssetFileInfoEx prefabIdentifierAsset = ResourceAssetsHelper.FindComponentOfType<PrefabIdentifier>(instance, (ulong)gameObjectPathId);
                    resourcesFile.reader.Position = prefabIdentifierAsset.absoluteFilePos + 28;

                    resourcesFile.reader.ReadCountStringInt32(); // Empty name
                    string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                    SubnauticaUwePrefabFactory.PrefabsByClassId[classId] = prefabs;

                    resourcesFile.reader.Position = prevPos; // Reset position
                }
            }
        }
    }
}
