using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        ErrorMessage.AddMessage($"Received a resync packet for bases with {packet.Entities.Count} entities");
        BuildingHandler.Main.StartCoroutine(ResyncEntities(packet.Entities));
    }

    public IEnumerator ResyncEntities(Dictionary<Entity, int> entities)
    {
        DateTimeOffset resyncStart = DateTimeOffset.Now;

        BuildingHandler.Main.StartResync(entities);
        yield return CoroutineHelper.SafelyYieldEnumerator(UpdateEntities<Base, BuildEntity>(
            entities.Keys.OfType<BuildEntity>().ToList(), OverwriteBase, (entity, reference) =>
            {
                return NitroxVector3.Distance(entity.LocalPosition, reference) < 0.001f;
            }
        ), exception => Log.Error($"Encountered an exception while resyncing BuildEntities:\n{exception}"));
        yield return CoroutineHelper.SafelyYieldEnumerator(UpdateEntities<Constructable, ModuleEntity>(
            entities.Keys.OfType<ModuleEntity>().ToList(), OverwriteModule, (entity, reference) =>
            {
                return NitroxVector3.Distance(entity.LocalPosition, reference) < 0.001f;
            }
        ), exception => Log.Error($"Encountered an exception while resyncing ModuleEntities:\n{exception}"));
        BuildingHandler.Main.Resyncing = false;

        DateTimeOffset resyncEnd = DateTimeOffset.Now;
        ErrorMessage.AddMessage($"Finished resyncing {entities.Count} entities, took {(resyncEnd - resyncStart).TotalSeconds}s");
    }

    public IEnumerator UpdateEntities<C,E>(List<E> entitiesToUpdate, Func<C, E, IEnumerator> overwrite, Func<E, NitroxVector3, bool> correspondingPredicate) where C : Component where E : Entity
    {
        List<C> unmarkedComponents = new();

        foreach (Transform childTransform in LargeWorldStreamer.main.globalRoot.transform)
        {
            if (childTransform.TryGetComponent(out C component))
            {
                if (component.TryGetNitroxId(out NitroxId id))
                {
                    E correspondingEntity = entitiesToUpdate.Find(entity => entity.Id.Equals(id));
                    if (correspondingEntity != null)
                    {
                        yield return CoroutineHelper.SafelyYieldEnumerator(overwrite(component, correspondingEntity), Log.Error);
                        entitiesToUpdate.Remove(correspondingEntity);
                        continue;
                    }
                }
                unmarkedComponents.Add(component);
            }
        }

        for (int i = entitiesToUpdate.Count - 1; i >= 0; i--)
        {
            E entity = entitiesToUpdate[i];
            C associatedComponent = unmarkedComponents.Find(c =>
                correspondingPredicate(entity, c.transform.localPosition.ToDto()));
            yield return CoroutineHelper.SafelyYieldEnumerator(overwrite(associatedComponent, entity), Log.Error);

            unmarkedComponents.Remove(associatedComponent);
            entitiesToUpdate.RemoveAt(i);
        }

        for (int i = unmarkedComponents.Count - 1; i >= 0; i--)
        {
            Log.Debug($"[{typeof(E)} RESYNC] Destroyed component {unmarkedComponents[i].gameObject}");
            GameObject.Destroy(unmarkedComponents[i].gameObject);
        }
        foreach (E entity in entitiesToUpdate)
        {
            Log.Debug($"[{typeof(E)} RESYNC] spawning entity {entity.Id}");
            yield return CoroutineHelper.SafelyYieldEnumerator(entities.SpawnAsync(entity), Log.Error);
        }
    }

    public IEnumerator OverwriteBase(Base @base, BuildEntity buildEntity)
    {
        Log.Debug($"[Base RESYNC] Overwriting base with id {buildEntity.Id}");
        ClearBaseChildren(@base);
        yield return BuildEntitySpawner.SetupBase(buildEntity, @base);
        yield return MoonpoolManager.RestoreMoonpools(buildEntity.ChildEntities.OfType<MoonpoolEntity>(), @base);
        foreach (MapRoomEntity mapRoomEntity in buildEntity.ChildEntities.OfType<MapRoomEntity>())
        {
            yield return InteriorPieceEntitySpawner.RestoreMapRoom(@base, mapRoomEntity);
        }
    }

    public IEnumerator OverwriteModule(Constructable constructable, ModuleEntity moduleEntity)
    {
        Log.Debug($"[Module RESYNC] Overwriting module with id {moduleEntity.Id}");
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
            if (child.GetComponent<IBaseModule>() != null || child.GetComponent<Constructable>())
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
