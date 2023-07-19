using System;
using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
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

    public BuildEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    public override IEnumerator SpawnAsync(BuildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a BuildEntity: {entity.Id}");
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject) && gameObject)
        {
            Log.Error("Trying to respawn an already spawned Base without a proper resync process.");
            yield break;
        }

        DateTimeOffset beginTime = DateTimeOffset.Now;
        GameObject newBase = UnityEngine.Object.Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, entity.LocalPosition.ToUnity(), entity.LocalRotation.ToUnity(), entity.LocalScale.ToUnity(), false);
        if (LargeWorld.main)
        {
            LargeWorld.main.streamer.cellManager.RegisterEntity(newBase);
        }
        Base @base = newBase.GetComponent<Base>();
        if (!@base)
        {
            Log.Debug("No Base component found");
            yield break;
        }
        yield return SetupBase(entity, @base, result);
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug(string.Format("Took {0}ms to create the Base", (endTime - beginTime).TotalMilliseconds));

        yield return entities.SpawnAsync(entity.ChildEntities.OfType<PlayerWorldEntity>());
        yield return MoonpoolManager.RestoreMoonpools(entity.ChildEntities.OfType<MoonpoolEntity>(), @base);
        foreach (MapRoomEntity mapRoomEntity in entity.ChildEntities.OfType<MapRoomEntity>())
        {
            yield return InteriorPieceEntitySpawner.RestoreMapRoom(@base, mapRoomEntity);
        }
        result.Set(@base.gameObject);
    }

    public override bool SpawnsOwnChildren(BuildEntity entity) => true;

    public static BuildEntity From(Base targetBase)
    {
        BuildEntity buildEntity = BuildEntity.MakeEmpty();
        if (targetBase.TryGetNitroxId(out NitroxId baseId))
        {
            buildEntity.Id = baseId;
        }

        buildEntity.LocalPosition = targetBase.transform.localPosition.ToDto();
        buildEntity.LocalRotation = targetBase.transform.localRotation.ToDto();
        buildEntity.LocalScale = targetBase.transform.localScale.ToDto();

        buildEntity.BaseData = GetBaseData(targetBase);
        buildEntity.ChildEntities.AddRange(BuildUtils.GetChildEntities(targetBase, baseId));

        return buildEntity;
    }

    public static BaseData GetBaseData(Base targetBase)
    {
        Func<byte[], byte[]> c = SerializationHelper.CompressBytes;

        BaseData baseData = new();
        if (targetBase.baseShape != null)
        {
            baseData.BaseShape = targetBase.baseShape.ToInt3().ToDto();
        }
        if (targetBase.faces != null)
        {
            baseData.Faces = c(Array.ConvertAll(targetBase.faces, faceType => (byte)faceType));
        }
        if (targetBase.cells != null)
        {
            baseData.Cells = c(Array.ConvertAll(targetBase.cells, cellType => (byte)cellType));
        }
        if (targetBase.links != null)
        {
            baseData.Links = c(targetBase.links);
            baseData.PrecompressionSize = targetBase.links.Length;
        }
        if (targetBase.cellOffset != null)
        {
            baseData.CellOffset = targetBase.cellOffset.ToDto();
        }
        if (targetBase.masks != null)
        {
            baseData.Masks = c(targetBase.masks);
        }
        if (targetBase.isGlass != null)
        {
            baseData.IsGlass = c(Array.ConvertAll(targetBase.isGlass, isGlass => isGlass ? (byte)1 : (byte)0));
        }
        if (targetBase.anchor != null)
        {
            baseData.Anchor = targetBase.anchor.ToDto();
        }
        return baseData;
    }

    public static void ApplyBaseData(BaseData baseData, Base @base)
    {
        Func<byte[], int, byte[]> d = SerializationHelper.DecompressBytes;
        int size = baseData.PrecompressionSize;

        if (baseData.BaseShape != null)
        {
            @base.baseShape = new(); // Reset it so that the following instruction is understood as a change
            @base.SetSize(baseData.BaseShape.ToUnity());
        }
        if (baseData.Faces != null)
        {
            @base.faces = Array.ConvertAll(d(baseData.Faces, size * 6), faceType => (Base.FaceType)faceType);
        }
        if (baseData.Cells != null)
        {
            @base.cells = Array.ConvertAll(d(baseData.Cells, size), cellType => (Base.CellType)cellType);
        }
        if (baseData.Links != null)
        {
            @base.links = d(baseData.Links, size);
        }
        if (baseData.CellOffset != null)
        {
            @base.cellOffset = new(baseData.CellOffset.ToUnity());
        }
        if (baseData.Masks != null)
        {
            @base.masks = d(baseData.Masks, size);
        }
        if (baseData.IsGlass != null)
        {
            @base.isGlass = Array.ConvertAll(d(baseData.IsGlass, size), num => num == 1);
        }
        if (baseData.Anchor != null)
        {
            @base.anchor = new(baseData.Anchor.ToUnity());
        }
    }

    public static IEnumerator SetupBase(BuildEntity buildEntity, Base @base, TaskResult<Optional<GameObject>> result = null)
    {
        GameObject baseObject = @base.gameObject;

        NitroxEntity.SetNewId(@base.gameObject, buildEntity.Id);
        ApplyBaseData(buildEntity.BaseData, @base);

        foreach (Entity childEntity in buildEntity.ChildEntities)
        {
            if (childEntity is InteriorPieceEntity || (childEntity is ModuleEntity && childEntity is not GhostEntity))
            {
                yield return Resolve<Entities>().SpawnAsync(childEntity);
            }
        }

        baseObject.SetActive(true);

        foreach (GhostEntity childGhostEntity in buildEntity.ChildEntities.OfType<GhostEntity>())
        {
            yield return GhostEntitySpawner.RestoreGhost(@base.transform, childGhostEntity);
        }

        @base.OnProtoDeserialize(null);
        @base.deserializationFinished = false;
        @base.FinishDeserialization();
        result?.Set(baseObject);
    }
}
