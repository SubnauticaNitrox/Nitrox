using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers.Abstract;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;
using NitroxServer_Subnautica.Serialization.ResourceAssets;
using NitroxServer.UnityStubs;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers
{
    public class ReefbackSlotsDataParser : MonoBehaviourParser
    {
        public override void Parse(AssetsFileInstance instance, long gameObjectPathId, ResourceAssets resourceAssets)
        {
            AssetsFile resourcesFile = instance.file;
            resourcesFile.reader.Align();
            uint reefbackPlantsDataSize = resourcesFile.reader.ReadUInt32();
            for (int i = 0; i < reefbackPlantsDataSize; i++)
            {
                resourcesFile.reader.ReadCountStringInt32();
                resourcesFile.reader.Align();
                uint plantsDataSize = resourcesFile.reader.ReadUInt32();
                List<ReefbackSpawnData.ReefbackEntity> plantPrefabs = new List<ReefbackSpawnData.ReefbackEntity>();

                for (int p = 0; p < plantsDataSize; p++)
                {
                    uint fileId = (uint)resourcesFile.reader.ReadInt32();
                    ulong pathId = (ulong)resourcesFile.reader.ReadInt64(); // PathId

                    AssetFileInfoEx asset = ResourceAssetsHelper.FindComponentOfType<PrefabIdentifier>(instance, pathId);
                    ulong prevPos = resourcesFile.reader.Position;

                    resourcesFile.reader.Position = asset.absoluteFilePos + 28;

                    resourcesFile.reader.ReadCountStringInt32(); // Empty name
                    string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                    resourcesFile.reader.Position = prevPos;
                    asset = ResourceAssetsHelper.FindTransform(instance, pathId);
                    prevPos = resourcesFile.reader.Position;
                    resourcesFile.reader.Position = asset.absoluteFilePos + 12;

                    Quaternion rotation = new Quaternion(
                        resourcesFile.reader.ReadSingle(), // Quaternion X
                        resourcesFile.reader.ReadSingle(), // Quaternion Y
                        resourcesFile.reader.ReadSingle(), // Quaternion Z
                        resourcesFile.reader.ReadSingle()); // Quaternion W

                    Vector3 position = new Vector3(
                        resourcesFile.reader.ReadSingle(), // Position X
                        resourcesFile.reader.ReadSingle(), // Position Y
                        resourcesFile.reader.ReadSingle()); // Position Z

                    Vector3 scale = new Vector3(
                        resourcesFile.reader.ReadSingle(), // Scale X
                        resourcesFile.reader.ReadSingle(), // Scale Y
                        resourcesFile.reader.ReadSingle()); // Scale Z

                    resourcesFile.reader.Position = prevPos;

                    plantPrefabs.Add(new ReefbackSpawnData.ReefbackEntity()
                    {
                        ClassId = classId,
                        Position = position,
                        Scale = scale,
                        Rotation = rotation
                    });
                }

                float probability = resourcesFile.reader.ReadSingle();
                float x = resourcesFile.reader.ReadSingle();
                float y = resourcesFile.reader.ReadSingle();
                float z = resourcesFile.reader.ReadSingle();

                foreach (ReefbackSpawnData.ReefbackEntity plant in plantPrefabs)
                {
                    ReefbackSpawnData.SpawnablePlants.Add(new ReefbackSpawnData.ReefbackEntity
                    {
                        ClassId = plant.ClassId,
                        Probability = probability,
                        Rotation = plant.Rotation,
                        Position = plant.Position,
                        Scale = plant.Scale
                    });
                }
            }

            uint reefbackCreatureSize = resourcesFile.reader.ReadUInt32();
            for (int i = 0; i < reefbackCreatureSize; i++)
            {

                resourcesFile.reader.ReadCountStringInt32(); // Name
                resourcesFile.reader.Align();
                uint fileId = (uint)resourcesFile.reader.ReadInt32();
                ulong pathId = (ulong)resourcesFile.reader.ReadInt64(); // PathId
                ReefbackSpawnData.ReefbackEntity creaturePrefab = new ReefbackSpawnData.ReefbackEntity();
                AssetFileInfoEx asset = ResourceAssetsHelper.FindComponentOfType<PrefabIdentifier>(instance, pathId);
                ulong prevPos = resourcesFile.reader.Position;
                resourcesFile.reader.Position = asset.absoluteFilePos + 28;

                resourcesFile.reader.ReadCountStringInt32(); // Empty name
                string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                resourcesFile.reader.Position = prevPos;
                asset = ResourceAssetsHelper.FindTransform(instance, pathId);
                prevPos = resourcesFile.reader.Position;
                resourcesFile.reader.Position = asset.absoluteFilePos + 12;

                Quaternion rotation = new Quaternion(
                    resourcesFile.reader.ReadSingle(), // Quaternion X
                    resourcesFile.reader.ReadSingle(), // Quaternion Y
                    resourcesFile.reader.ReadSingle(), // Quaternion Z
                    resourcesFile.reader.ReadSingle()); // Quaternion W

                Vector3 position = new Vector3(
                    resourcesFile.reader.ReadSingle(), // Position X
                    resourcesFile.reader.ReadSingle(), // Position Y
                    resourcesFile.reader.ReadSingle()); // Position Z

                Vector3 scale = new Vector3(
                    resourcesFile.reader.ReadSingle(), // Scale X
                    resourcesFile.reader.ReadSingle(), // Scale Y
                    resourcesFile.reader.ReadSingle()); // Scale Z

                creaturePrefab = new ReefbackSpawnData.ReefbackEntity
                {
                    ClassId = classId,
                    Position = position,
                    Scale = scale,
                    Rotation = rotation,
                };

                resourcesFile.reader.Position = prevPos;


                int minNumber = resourcesFile.reader.ReadInt32();
                int maxNumber = resourcesFile.reader.ReadInt32();
                float probability = resourcesFile.reader.ReadSingle();

                ReefbackSpawnData.SpawnableCreatures.Add(new ReefbackSpawnData.ReefbackEntity()
                {
                    ClassId = creaturePrefab.ClassId,
                    Position = creaturePrefab.Position,
                    Scale = creaturePrefab.Scale,
                    Rotation = creaturePrefab.Rotation,
                    Probability = probability,
                    MinNumber = minNumber,
                    MaxNumber = maxNumber
                });
            }
        }
    }
}
