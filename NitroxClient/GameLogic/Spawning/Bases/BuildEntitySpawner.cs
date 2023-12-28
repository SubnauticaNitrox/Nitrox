using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class BuildEntitySpawner : EntitySpawner<BuildEntity>
{
    private readonly Entities entities;
    private readonly BaseLeakEntitySpawner baseLeakEntitySpawner;

    public BuildEntitySpawner(Entities entities, BaseLeakEntitySpawner baseLeakEntitySpawner)
    {
        this.entities = entities;
        this.baseLeakEntitySpawner = baseLeakEntitySpawner;
    }

    protected override IEnumerator SpawnAsync(BuildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject) && gameObject)
        {
            Log.Error("Trying to respawn an already spawned Base without a proper resync process.");
            yield break;
        }

#if DEBUG
        Stopwatch stopwatch = Stopwatch.StartNew();
#endif
        GameObject newBase = UnityEngine.Object.Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, entity.Transform.LocalPosition.ToUnity(), entity.Transform.LocalRotation.ToUnity(), entity.Transform.LocalScale.ToUnity(), false);
        if (LargeWorld.main)
        {
            LargeWorld.main.streamer.cellManager.RegisterEntity(newBase);
        }
        Base @base = newBase.GetComponent<Base>();
        yield return SetupBase(entity, @base, entities, result);
#if DEBUG
        Log.Verbose($"Took {stopwatch.ElapsedMilliseconds}ms to create the Base");
#endif
        yield return entities.SpawnBatchAsync(entity.ChildEntities.OfType<PlayerWorldEntity>().ToList<Entity>());
        yield return MoonpoolManager.RestoreMoonpools(entity.ChildEntities.OfType<MoonpoolEntity>(), @base);

        TaskResult<Optional<GameObject>> childResult = new();
        bool atLeastOneLeak = false;
        foreach (Entity childEntity in entity.ChildEntities)
        {
            switch (childEntity)
            {
                case MapRoomEntity mapRoomEntity:
                    yield return InteriorPieceEntitySpawner.RestoreMapRoom(@base, mapRoomEntity);
                    break;
                case BaseLeakEntity baseLeakEntity:
                    atLeastOneLeak = true;
                    yield return baseLeakEntitySpawner.SpawnAsync(baseLeakEntity, childResult);
                    break;
            }
        }
        if (atLeastOneLeak)
        {
            BaseHullStrength baseHullStrength = @base.GetComponent<BaseHullStrength>();
            ErrorMessage.AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", baseHullStrength.totalStrength));
        }

        result.Set(@base.gameObject);
    }

    protected override bool SpawnsOwnChildren(BuildEntity entity) => true;

    public static BuildEntity From(Base targetBase, EntityMetadataManager entityMetadataManager)
    {
        BuildEntity buildEntity = BuildEntity.MakeEmpty();
        if (targetBase.TryGetNitroxId(out NitroxId baseId))
        {
            buildEntity.Id = baseId;
        }

        buildEntity.Transform = targetBase.transform.ToLocalDto();

        buildEntity.BaseData = GetBaseData(targetBase);
        buildEntity.ChildEntities.AddRange(BuildUtils.GetChildEntities(targetBase, baseId, entityMetadataManager));

        return buildEntity;
    }

    public static BaseData GetBaseData(Base targetBase)
    {
        return new BaseData()
        {
            BaseShape = targetBase.baseShape.ToInt3().ToDto(),
            Faces = BaseSerializationHelper.CompressData(targetBase.faces, faceType => (byte)faceType),
            Cells = BaseSerializationHelper.CompressData(targetBase.cells, cellType => (byte)cellType),
            Links = BaseSerializationHelper.CompressBytes(targetBase.links),
            PreCompressionSize = targetBase.links.Length,
            CellOffset = targetBase.cellOffset.ToDto(),
            Masks = BaseSerializationHelper.CompressBytes(targetBase.masks),
            IsGlass = BaseSerializationHelper.CompressData(targetBase.isGlass, isGlass => isGlass ? (byte)1 : (byte)0),
            Anchor = targetBase.anchor.ToDto()
        };
    }

    public static void ApplyBaseData(BaseData baseData, Base @base)
    {
        int size = baseData.PreCompressionSize;

        @base.baseShape = new(); // Reset it so that the following instruction is understood as a change
        @base.SetSize(baseData.BaseShape.ToUnity());
        @base.faces = BaseSerializationHelper.DecompressData(baseData.Faces, size * 6, faceType => (Base.FaceType)faceType);
        @base.cells = BaseSerializationHelper.DecompressData(baseData.Cells, size, cellType => (Base.CellType)cellType);
        @base.links = BaseSerializationHelper.DecompressBytes(baseData.Links, size);
        @base.cellOffset = new(baseData.CellOffset.ToUnity());
        @base.masks = BaseSerializationHelper.DecompressBytes(baseData.Masks, size);
        @base.isGlass = BaseSerializationHelper.DecompressData(baseData.IsGlass, size, num => num == 1);
        @base.anchor = new(baseData.Anchor.ToUnity());
    }

    public static IEnumerator SetupBase(BuildEntity buildEntity, Base @base, Entities entities, TaskResult<Optional<GameObject>> result = null)
    {
        GameObject baseObject = @base.gameObject;

        NitroxEntity.SetNewId(@base.gameObject, buildEntity.Id);
        ApplyBaseData(buildEntity.BaseData, @base);

        // Ghosts need an active base to be correctly spawned onto it
        // While the rest must be spawned earlier for the base to load correctly (mostly InteriorPieceEntity)
        // Which is why the spawn loops are separated by the SetActive instruction
        // NB: We aim at spawning very precise entity types (InteriorPieceEntity, ModuleEntity and GlobalRootEntity)
        // Thus we use GetType() == instead of "is GlobalRootEntity" so that derived types from it aren't selected
        List<GhostEntity> ghostChildrenEntities = new();
        foreach (Entity childEntity in buildEntity.ChildEntities)
        {
            if (childEntity is InteriorPieceEntity || childEntity is ModuleEntity ||
                childEntity.GetType() == typeof(GlobalRootEntity))
            {
                switch (childEntity)
                {
                    case GhostEntity ghostEntity:
                        ghostChildrenEntities.Add(ghostEntity);
                        continue;
                }

                yield return entities.SpawnEntityAsync(childEntity, true);
            }
        }

        baseObject.SetActive(true);

        foreach (GhostEntity childGhostEntity in ghostChildrenEntities)
        {
            yield return GhostEntitySpawner.RestoreGhost(@base.transform, childGhostEntity);
        }

        @base.OnProtoDeserialize(null);
        @base.deserializationFinished = false;
        @base.FinishDeserialization();
        result?.Set(baseObject);
    }
}
