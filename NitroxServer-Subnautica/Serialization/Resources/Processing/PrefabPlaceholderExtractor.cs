using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours;

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

                List<PrefabAsset> spawnablePrefabs = new List<PrefabAsset>();
                               
                foreach (AssetIdentifier prefabPlaceholderId in prefabPlaceholders)
                {
                    PrefabPlaceholderAsset prefabPlaceholderAsset = PrefabPlaceholderParser.PrefabPlaceholderIdToPlaceholderAsset[prefabPlaceholderId];
                    spawnablePrefabs.Add(CreatePrefabAsset(prefabPlaceholderAsset.GameObjectIdentifier, prefabPlaceholderAsset.ClassId));
                }

                GameObjectAsset gameObject = GameObjectAssetParser.GameObjectsByAssetId[placeholderGroup.Identifier];
                TransformAsset localTransform = GetTransform(gameObject);

                List<PrefabAsset> existingPrefabs = GetChildPrefabs(localTransform);

                resourceAssets.PrefabPlaceholderGroupsByGroupClassId[placeholderGroupClassId] = new PrefabPlaceholdersGroupAsset(spawnablePrefabs, existingPrefabs);
            }
        }

        private PrefabAsset CreatePrefabAsset(AssetIdentifier gameObjectId, string classId)
        {
            GameObjectAsset gameObject = GameObjectAssetParser.GameObjectsByAssetId[gameObjectId];
            TransformAsset localTransform = GetTransform(gameObject);

            Optional<NitroxEntitySlot> entitySlot = (classId != null) ? GetEntitySlot(classId) : Optional.Empty;
            List<PrefabAsset> children = GetChildPrefabs(localTransform);

            return new PrefabAsset(gameObject.Name, classId, localTransform, entitySlot, children);
        }

        private List<PrefabAsset> GetChildPrefabs(TransformAsset parentTransform)
        {
            List<PrefabAsset> children = new List<PrefabAsset>();

            foreach (AssetIdentifier childTransformId in TransformAssetParser.ChildrenIdsByParentId[parentTransform.Identifier])
            {
                TransformAsset childTransform = TransformAssetParser.TransformsByAssetId[childTransformId];

                string childClassId;

                PrefabIdentifierParser.ClassIdByGameObjectId.TryGetValue(childTransform.GameObjectIdentifier, out childClassId);

                children.Add(CreatePrefabAsset(childTransform.GameObjectIdentifier, childClassId));
            }

            return children;
        }

        private Optional<NitroxEntitySlot> GetEntitySlot(string classId)
        {
            AssetIdentifier prefabId = PrefabIdentifierParser.GameObjectIdByClassId[classId];
            GameObjectAsset gameObject = GameObjectAssetParser.GameObjectsByAssetId[prefabId];
            NitroxEntitySlot entitySlot;

            EntitySlotParser.EntitySlotsByIdentifier.TryGetValue(gameObject.Identifier, out entitySlot);

            return Optional.OfNullable(entitySlot);
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
