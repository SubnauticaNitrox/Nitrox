using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class BuildingResyncProcessor : ClientPacketProcessor<BuildingResync>
{
    private readonly Entities entities;

    public BuildingResyncProcessor(Entities entities)
    {
        this.entities = entities;
    }

    public override void Process(BuildingResync packet)
    {
        if (!BuildingHandler.Main)
        {
            return;
        }

        BuildingHandler.Main.StartCoroutine(ResyncBuildingEntities(packet.BuildEntities, packet.ModuleEntities));
    }

    public IEnumerator ResyncBuildingEntities(Dictionary<BuildEntity, int> buildEntities, Dictionary<ModuleEntity, int> moduleEntities)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        BuildingHandler.Main.StartResync(buildEntities);
        yield return UpdateEntities<Base, BuildEntity>(buildEntities.Keys.ToList(), OverwriteBase, IsInCloseProximity).OnYieldError(exception => Log.Error(exception, $"Encountered an exception while resyncing BuildEntities"));

        BuildingHandler.Main.StartResync(moduleEntities);
        yield return UpdateEntities<Constructable, ModuleEntity>(moduleEntities.Keys.ToList(), OverwriteModule, IsInCloseProximity).OnYieldError(exception => Log.Error(exception, $"Encountered an exception while resyncing ModuleEntities"));
        BuildingHandler.Main.StopResync();

        stopwatch.Stop();

        int totalEntities = buildEntities.Count + moduleEntities.Count;
        Log.InGame(Language.main.Get("Nitrox_FinishedResyncRequest").Replace("{TIME}", stopwatch.ElapsedMilliseconds.ToString()).Replace("{COUNT}", totalEntities.ToString()));
    }

    private bool IsInCloseProximity<C>(WorldEntity entity, C componentInWorld) where C : Component
    {
        return Vector3.Distance(entity.Transform.Position.ToUnity(), componentInWorld.transform.position) < 0.001f;
    }

    /// <summary>
    ///     Tries to overwrite components of the provided type found in GlobalRoot's hierarchy by the provided list of entities to update.
    ///     If no component is found to be corresponding to a provided entity, the entity will be spawned independently.
    ///     Other components of the provided type which weren't updated shall be destroyed.
    /// </summary>
    /// <remarks>
    ///     The provided list is modified by the function. Make sure it's not used somewhere else.
    /// </remarks>
    /// <typeparam name="C">The Unity component to be looked for</typeparam>
    /// <typeparam name="E">The GlobalRootEntity type which will be updated</typeparam>
    /// <param name="overwrite">A function to overwrite a given component by a given entity</param>
    /// <param name="correspondingPredicate">
    /// Predicate to determine if an entity can overwrite the GameObject of the provided component.
    /// </param>
    public IEnumerator UpdateEntities<C,E>(List<E> entitiesToUpdate, Func<C, E, IEnumerator> overwrite, Func<E, C, bool> correspondingPredicate) where C : Component where E : GlobalRootEntity
    {
        List<C> unmarkedComponents = new();
        Dictionary<NitroxId, E> entitiesToUpdateById = entitiesToUpdate.ToDictionary(e => e.Id);

        foreach (Transform childTransform in LargeWorldStreamer.main.globalRoot.transform)
        {
            if (childTransform.TryGetComponent(out C component))
            {
                if (component.TryGetNitroxId(out NitroxId id) && entitiesToUpdateById.TryGetValue(id, out E correspondingEntity))
                {
                    yield return overwrite(component, correspondingEntity).OnYieldError(Log.Error);
                    entitiesToUpdate.Remove(correspondingEntity);
                    continue;
                }
                unmarkedComponents.Add(component);
            }
        }

        for (int i = entitiesToUpdate.Count - 1; i >= 0; i--)
        {
            E entity = entitiesToUpdate[i];
            C associatedComponent = unmarkedComponents.Find(c =>
                correspondingPredicate(entity, c));
            yield return overwrite(associatedComponent, entity).OnYieldError(Log.Error);

            unmarkedComponents.Remove(associatedComponent);
            entitiesToUpdate.RemoveAt(i);
        }

        for (int i = unmarkedComponents.Count - 1; i >= 0; i--)
        {
            Log.Info($"[{typeof(E)} RESYNC] Destroyed GameObject {unmarkedComponents[i].gameObject}");
            GameObject.Destroy(unmarkedComponents[i].gameObject);
        }
        foreach (E entity in entitiesToUpdate)
        {
            Log.Info($"[{typeof(E)} RESYNC] spawning entity {entity.Id}");
            yield return entities.SpawnEntityAsync(entity).OnYieldError(Log.Error);
        }
    }

    public IEnumerator OverwriteBase(Base @base, BuildEntity buildEntity)
    {
        Log.Info($"[Base RESYNC] Overwriting base with id {buildEntity.Id}");
        ClearBaseChildren(@base);
        yield return BuildEntitySpawner.SetupBase(buildEntity, @base, entities);
        yield return MoonpoolManager.RestoreMoonpools(buildEntity.ChildEntities.OfType<MoonpoolEntity>(), @base);
        yield return entities.SpawnBatchAsync(buildEntity.ChildEntities.OfType<PlayerWorldEntity>().ToList<Entity>(), true, false);
        foreach (MapRoomEntity mapRoomEntity in buildEntity.ChildEntities.OfType<MapRoomEntity>())
        {
            yield return InteriorPieceEntitySpawner.RestoreMapRoom(@base, mapRoomEntity);
        }
    }

    public IEnumerator OverwriteModule(Constructable constructable, ModuleEntity moduleEntity)
    {
        Log.Info($"[Module RESYNC] Overwriting module with id {moduleEntity.Id}");
        ModuleEntitySpawner.ApplyModuleData(moduleEntity, constructable.gameObject);
        yield break;
    }

    /// <summary>
    /// Destroys manually ghosts, modules, interior pieces and vehicles of a base
    /// </summary>
    /// <remarks>
    /// This is the destructive way of clearing the base, if the base isn't modified consequently, IBaseModuleGeometry under the base cells may start spamming errors.
    /// </remarks>
    public static void ClearBaseChildren(Base @base)
    {
        for (int i = @base.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = @base.transform.GetChild(i);
            if (child.GetComponent<IBaseModule>().AliveOrNull() || child.GetComponent<Constructable>())
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
        foreach (VehicleDockingBay vehicleDockingBay in @base.GetComponentsInChildren<VehicleDockingBay>(true))
        {
            if (vehicleDockingBay.dockedVehicle)
            {
                UnityEngine.Object.Destroy(vehicleDockingBay.dockedVehicle.gameObject);
                vehicleDockingBay.SetVehicleUndocked();
            }
        }
    }
}
