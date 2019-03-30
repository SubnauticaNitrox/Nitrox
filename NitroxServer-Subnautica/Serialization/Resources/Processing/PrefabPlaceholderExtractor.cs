using System;
using System.Collections.Generic;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.GameObjectAssetParser;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.TransformAssetParser;

namespace NitroxServer_Subnautica.Serialization.Resources.Processing
{
    public class PrefabPlaceholderExtractor
    {
        public void LoadInto(ResourceAssets resourceAssets)
        {
            foreach (GameObjectAsset gameObjectAsset in GameObjectAssetParser.GameObjectAssets)
            {
                string prefabPlaceholder;

                if (!PrefabPlaceholdersGroupParser.PrefabByGameObjectId.TryGetValue(gameObjectAsset.Identifier, out prefabPlaceholder))
                {
                    continue;
                }

                string classId;

                if (!PrefabIdentifierParser.ClassIdByGameObjectId.TryGetValue(gameObjectAsset.Identifier, out classId))
                {
                    throw new Exception("All prefab placeholders should have a class id " + gameObjectAsset.Identifier);
                }

                TransformAsset transform = FindTransform(gameObjectAsset);

                List<PrefabAsset> prefabs;

                if(!resourceAssets.PrefabsByClassId.TryGetValue(classId, out prefabs))
                {
                    prefabs = new List<PrefabAsset>();
                    resourceAssets.PrefabsByClassId[classId] = prefabs;
                }

                prefabs.Add(new PrefabAsset(classId, transform));
            }
        }

        private TransformAsset FindTransform(GameObjectAsset gameObjectAsset)
        {
            foreach (AssetIdentifier componentIdentifier in gameObjectAsset.Components)
            {
                TransformAsset transform;

                if (TransformAssetParser.TransformsByAssetId.TryGetValue(componentIdentifier, out transform))
                {
                    return transform;
                }
            }

            throw new Exception("No transform found for " + gameObjectAsset.Identifier);
        }
    }
}
