using System.Collections;
using Nitrox.Model.DataStructures;
using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata.Processor;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class EscapePodEntitySpawner : SyncEntitySpawner<EscapePodEntity>
{
    /*
     * When creating additional escape pods (multiple users with multiple pods)
     * we want to suppress the escape pod's awake method so it doesn't override
     * EscapePod.main to the new escape pod.
     */
    public static bool SuppressEscapePodAwakeMethod;

    private readonly LocalPlayer localPlayer;

    public EscapePodEntitySpawner(LocalPlayer localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    protected override IEnumerator SpawnAsync(EscapePodEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        return null;
    }

    protected override bool SpawnSync(EscapePodEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SuppressEscapePodAwakeMethod = true;

        GameObject escapePod = CreateNewEscapePod(entity);

        SuppressEscapePodAwakeMethod = false;

        result.Set(Optional.Of(escapePod));
        return true;
    }

    protected override bool SpawnsOwnChildren(EscapePodEntity entity) => false;

    private GameObject CreateNewEscapePod(EscapePodEntity escapePodEntity)
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

        pod.anchorPosition = escapePod.transform.position = escapePodEntity.Transform.Position.ToUnity();

        pod.ForceSkyApplier();
        pod.escapePodCinematicControl.StopAll();

        // Player is not new and has completed the intro cinematic. If not EscapePod repair status is handled by the intro cinematic.
        if (escapePodEntity.Metadata is EscapePodMetadata metadata && localPlayer.IntroCinematicMode == IntroCinematicMode.COMPLETED)
        {
            using FMODSoundSuppressor soundSuppressor = FMODSystem.SuppressSubnauticaSounds();
            using PacketSuppressor<EntityMetadataUpdate> packetSuppressor = PacketSuppressor<EntityMetadataUpdate>.Suppress();

            Radio radio = pod.radioSpawner.spawnedObj.GetComponent<Radio>();
            EscapePodMetadataProcessor.ProcessInitialSyncMetadata(pod, radio, metadata);
            // NB: Entities.SpawnBatchAsync (which is the function calling the current spawner)
            // will still apply the metadata another time but we don't care as it's not destructive
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
}
