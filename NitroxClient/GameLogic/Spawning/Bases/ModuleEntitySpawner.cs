using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class ModuleEntitySpawner : EntitySpawner<ModuleEntity>
{
    private readonly Entities entities;

    public ModuleEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    protected override IEnumerator SpawnAsync(ModuleEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject) && gameObject)
        {
            Log.Error("Trying to respawn an already spawned module without a proper resync process.");
            yield break;
        }
        Transform parent = BuildingHandler.GetParentOrGlobalRoot(entity.ParentId);

        yield return RestoreModule(parent, entity, result);

        if (!result.Get().HasValue)
        {
            Log.Error($"Module couldn't be spawned {entity}");
            yield break;
        }
        GameObject moduleObject = result.Get().Value;
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(moduleObject);
        if (!opContainer.HasValue)
        {
            yield break;
        }

        yield return entities.SpawnBatchAsync(entity.ChildEntities.OfType<InventoryItemEntity>().ToList<Entity>(), true);

        if (moduleObject.TryGetComponent(out PowerSource powerSource))
        {
            // TODO: Have synced/restored power
            powerSource.SetPower(powerSource.maxPower);
        }
    }

    protected override bool SpawnsOwnChildren(ModuleEntity entity) => true;

    public static IEnumerator RestoreModule(Transform parent, ModuleEntity moduleEntity, TaskResult<Optional<GameObject>> result = null)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: moduleEntity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(moduleEntity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for module of ClassId {moduleEntity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        GameObject moduleObject = UnityEngine.Object.Instantiate(prefab);
        Transform moduleTransform = moduleObject.transform;
        moduleTransform.parent = parent;
        moduleTransform.localPosition = moduleEntity.Transform.LocalPosition.ToUnity();
        moduleTransform.localRotation = moduleEntity.Transform.LocalRotation.ToUnity();
        moduleTransform.localScale = moduleEntity.Transform.LocalScale.ToUnity();
        ApplyModuleData(moduleEntity, moduleObject, result);
        yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, moduleEntity.Id);
    }

    public static void ApplyModuleData(ModuleEntity moduleEntity, GameObject moduleObject, TaskResult<Optional<GameObject>> result = null)
    {
        Constructable constructable = moduleObject.GetComponent<Constructable>();
        constructable.SetIsInside(moduleEntity.IsInside);
        if (moduleEntity.IsInside)
        {
            SkyEnvironmentChanged.Send(moduleObject, moduleObject.GetComponentInParent<SubRoot>(true));
        }
        else
        {
            SkyEnvironmentChanged.Send(moduleObject, (Component)null);
        }
        constructable.constructedAmount = moduleEntity.ConstructedAmount;
        constructable.SetState(moduleEntity.ConstructedAmount >= 1f, false);
        constructable.UpdateMaterial();
        NitroxEntity.SetNewId(moduleObject, moduleEntity.Id);
        result?.Set(moduleObject);
    }

    public static void FillObject(ModuleEntity moduleEntity, Constructable constructable)
    {
        moduleEntity.ClassId = constructable.GetComponent<PrefabIdentifier>().ClassId;

        if (constructable.TryGetNitroxId(out NitroxId entityId))
        {
            moduleEntity.Id = entityId;
        }
        if (constructable.TryGetComponentInParent(out Base parentBase, true) &&
            parentBase.TryGetNitroxId(out NitroxId parentId))
        {
            moduleEntity.ParentId = parentId;
        }
        moduleEntity.Transform = constructable.transform.ToLocalDto();
        moduleEntity.TechType = constructable.techType.ToDto();
        moduleEntity.ConstructedAmount = constructable.constructedAmount;
        moduleEntity.IsInside = constructable.isInside;
    }

    public static ModuleEntity From(Constructable constructable)
    {
        ModuleEntity module = ModuleEntity.MakeEmpty();
        FillObject(module, constructable);
        return module;
    }
}
