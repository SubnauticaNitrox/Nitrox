using System;
using System.Collections.Generic;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.GameObjectAssetParser;
using static NitroxServer_Subnautica.Serialization.Resources.Parsers.TransformAssetParser;

namespace NitroxServer_Subnautica.Serialization.Resources.Processing
{
    /**
     * Subnautica spawns certain types of items using PrefabPlaceholderGroups.  These internally have a 
     * list of PrefabPlaceholders that can spawn an item. When Nitrox spawns items, it will need to do a 
     * lookup every time it identifies a PrefabPlaceholderGroup to replace it with the resulting entities.
     * We surpress the client from doing this as we don't want virtual entities that the server does not
     * know about.
     */
    public class PrefabPlaceholderExtractor
    {
        public void LoadInto(ResourceAssets resourceAssets)
        {
            foreach (GameObjectAsset placeholderGroup in GameObjectAssetParser.GameObjectsByAssetId.Values)
            {
                List<AssetIdentifier> prefabPlaceholders;

                if (!PrefabPlaceholdersGroupParser.PrefabPlaceholderIdsByGameObjectId.TryGetValue(placeholderGroup.Identifier, out prefabPlaceholders))
                {
                    continue;
                }

                string placeholderGroupClassId = PrefabIdentifierParser.ClassIdByGameObjectId[placeholderGroup.Identifier];

                List<PrefabAsset> prefabs;

                if (!resourceAssets.PlaceholderPrefabsByGroupClassId.TryGetValue(placeholderGroupClassId, out prefabs))
                {
                    prefabs = new List<PrefabAsset>();
                    resourceAssets.PlaceholderPrefabsByGroupClassId[placeholderGroupClassId] = prefabs;
                }
                
                foreach (AssetIdentifier prefabPlaceholderId in prefabPlaceholders)
                {
                    PrefabPlaceholderAsset prefabPlaceholderAsset = PrefabPlaceholderParser.PrefabPlaceholderIdToPlaceholderAsset[prefabPlaceholderId];
                    GameObjectAsset prefabPlaceholder = GameObjectAssetParser.GameObjectsByAssetId[prefabPlaceholderAsset.GameObjectIdentifier];                    
                    TransformAsset localTransform = GetTransform(prefabPlaceholder);

                    prefabs.Add(new PrefabAsset(prefabPlaceholderAsset.ClassId, localTransform));
                }
            }
        }

        private TransformAsset GetTransform(GameObjectAsset gameObjectAsset)
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
