using System.Collections;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class EscapePodWorldEntitySpawner : IWorldEntitySpawner
    {
        /*
         * When creating additional escape pods (multiple users with multiple pods)
         * we want to supress the escape pod's awake method so it doesn't override
         * EscapePod.main to the new escape pod.
         */
        public static bool SURPRESS_ESCAPE_POD_AWAKE_METHOD;

        private SimulationOwnership simulationOwnership;

        public EscapePodWorldEntitySpawner(SimulationOwnership simulationOwnership)
        {
            this.simulationOwnership = simulationOwnership;
        }

        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            if (entity is not EscapePodWorldEntity escapePodEntity)
            {
                result.Set(Optional.Empty);
                Log.Error($"Received incorrect entity type: {entity.GetType()}");
                yield break;
            }

            SURPRESS_ESCAPE_POD_AWAKE_METHOD = true;

            GameObject escapePod = CreateNewEscapePod(escapePodEntity);
            escapePod.EnsureComponent<MovementController>();
            simulationOwnership.RequestSimulationLock(entity.Id, SimulationLockType.TRANSIENT);

            SURPRESS_ESCAPE_POD_AWAKE_METHOD = false;

            result.Set(Optional.Of(escapePod));
        }

        private static GameObject CreateNewEscapePod(EscapePodWorldEntity escapePodEntity)
        {
            // TODO: When we want to implement multiple escape pods, instantiate the prefab. Backlog task: #1945
            //       This will require some work as instantiating the prefab as-is will not make it visible.
            //GameObject escapePod = Object.Instantiate(EscapePod.main.gameObject);
            
            GameObject escapePod = EscapePod.main.gameObject;
            UnityEngine.Component.DestroyImmediate(escapePod.GetComponent<NitroxEntity>()); // if template has a pre-existing NitroxEntity, remove.
            NitroxEntity.SetNewId(escapePod, escapePodEntity.Id);

            EntityMetadataProcessor.ApplyMetadata(escapePod, escapePodEntity.Metadata);

            escapePod.transform.position = escapePodEntity.Transform.Position.ToUnity();

            FixStartMethods(escapePod);

            // Start() isn't executed for the EscapePod, why? Idk, maybe because it's a scene...
            MultiplayerCinematicReference reference = escapePod.AddComponent<MultiplayerCinematicReference>();
            foreach (PlayerCinematicController controller in escapePod.GetComponentsInChildren<PlayerCinematicController>())
            {
                reference.AddController(controller);
            }

            return escapePod;
        }

        /// <summary>
        /// Start() isn't executed for the EscapePod and children (Why? Idk, maybe because it's a scene...) so we call the components here where we have patches in Start.
        /// </summary>
        private static void FixStartMethods(GameObject escapePod)
        {
            foreach (FMOD_CustomEmitter customEmitter in escapePod.GetComponentsInChildren<FMOD_CustomEmitter>())
            {
                customEmitter.Start();
            }

            foreach (FMOD_StudioEventEmitter studioEventEmitter in escapePod.GetComponentsInChildren<FMOD_StudioEventEmitter>())
            {
                studioEventEmitter.Start();
            }
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }

}
