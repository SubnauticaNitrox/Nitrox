using System.Collections;
using NitroxClient.GameLogic.Spawning.Metadata.Processor;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class EscapePodWorldEntitySpawner : IWorldEntitySpawner
{
    /*
     * When creating additional escape pods (multiple users with multiple pods)
     * we want to suppress the escape pod's awake method so it doesn't override
     * EscapePod.main to the new escape pod.
     */
    public static bool SuppressEscapePodAwakeMethod;

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not EscapePodWorldEntity escapePodEntity)
        {
            result.Set(Optional.Empty);
            Log.Error($"Received incorrect entity type: {entity.GetType()}");
            yield break;
        }

        SuppressEscapePodAwakeMethod = true;

        GameObject escapePod = CreateNewEscapePod(escapePodEntity);

        SuppressEscapePodAwakeMethod = false;

        result.Set(Optional.Of(escapePod));
    }

    private GameObject CreateNewEscapePod(EscapePodWorldEntity escapePodEntity)
    {
        // TODO: When we want to implement multiple escape pods, instantiate the prefab. Backlog task: #1945
        //       This will require some work as instantiating the prefab as-is will not make it visible.
        //GameObject escapePod = Object.Instantiate(EscapePod.main.gameObject);
        GameObject escapePod = EscapePod.main.gameObject;
        EscapePod pod = escapePod.GetComponent<EscapePod>();

        Object.DestroyImmediate(escapePod.GetComponent<NitroxEntity>()); // if template has a pre-existing NitroxEntity, remove.
        NitroxEntity.SetNewId(escapePod, escapePodEntity.Id);

        if (escapePod.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            Log.Error("Escape pod did not have a rigid body!");
        }

        escapePod.transform.position = escapePodEntity.Transform.Position.ToUnity();

        pod.ForceSkyApplier();
        pod.escapePodCinematicControl.StopAll();

        if (escapePodEntity.Metadata is EscapePodMetadata metadata)
        {
            Radio radio = pod.radioSpawner.spawnedObj.GetComponent<Radio>();
            EscapePodMetadataProcessor.ProcessInitialSyncMetadata(pod, radio, metadata);
        }
        else
        {
            Log.Error($"[{nameof(EscapePodWorldEntitySpawner)}] Metadata was not of type {nameof(EscapePodMetadata)}.");
        }

        FixStartMethods(escapePod);

        return escapePod;
    }

    /// <summary>
    /// Start() isn't executed for the EscapePod and children (Why? Idk, maybe because it's a scene...) so we call the components here where we have patches in Start.
    /// </summary>
    private static void FixStartMethods(GameObject escapePod)
    {
        foreach (FMOD_CustomEmitter customEmitter in escapePod.GetComponentsInChildren<FMOD_CustomEmitter>(true))
        {
            customEmitter.Start();
        }

        foreach (FMOD_StudioEventEmitter studioEventEmitter in escapePod.GetComponentsInChildren<FMOD_StudioEventEmitter>(true))
        {
            studioEventEmitter.Start();
        }

        MultiplayerCinematicReference reference = escapePod.EnsureComponent<MultiplayerCinematicReference>();
        foreach (PlayerCinematicController controller in escapePod.GetComponentsInChildren<PlayerCinematicController>(true))
        {
            reference.AddController(controller);
        }
    }

    public bool SpawnsOwnChildren() => false;
}
