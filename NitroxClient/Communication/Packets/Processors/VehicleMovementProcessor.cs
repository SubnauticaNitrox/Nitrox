using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : ClientPacketProcessor<VehicleMovement>
    {
        private PlayerManager remotePlayerManager;

        public VehicleMovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicleMovement.Guid);

            RemotePlayer player = remotePlayerManager.FindOrCreate(vehicleMovement.PlayerId);

            Vector3 remotePosition = vehicleMovement.Position;
            Vector3 remoteVelocity = vehicleMovement.Velocity;
            Quaternion remoteRotation = vehicleMovement.BodyRotation;

            Vehicle vehicle = null;
            SubRoot subRoot = null;
            if (opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();

                Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

                if (rigidbody != null)
                {
                    //todo: maybe toggle kinematic if jumping large distances?

                    /*
                     * For the cyclops, it is too intense for the game to lerp the entire structure every movement
                     * packet update.  Instead, we try to match the velocity.  Due to floating points not being
                     * precise, this will skew quickly.  To counter this, we apply micro adjustments each packet
                     * to get the simulation back in sync.  The adjustments will increase in size the larger the
                     * out of sync issue is.
                     *
                     * Besides, this causes the movement of the Cyclops, vehicles and player to be very fluid.
                     */

                    rigidbody.velocity = MovementHelper.GetCorrectedVelocity(remotePosition, remoteVelocity, gameObject, PlayerMovement.BROADCAST_INTERVAL);
                    rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(remoteRotation, gameObject, PlayerMovement.BROADCAST_INTERVAL);
                }
                else
                {
                    Console.WriteLine("Vehicle did not have a rigidbody!");
                }

                vehicle = gameObject.GetComponent<Vehicle>();
                subRoot = gameObject.GetComponent<SubRoot>();
            }
            else
            {
                CreateVehicleAt(player, vehicleMovement.TechType, vehicleMovement.Guid, remotePosition, remoteRotation);
            }
            player.SetVehicle(vehicle);
            player.SetSubRoot(subRoot);
            player.SetPilotingChair(subRoot?.GetComponentInChildren<PilotingChair>());

            player.animationController.UpdatePlayerAnimations = false;
        }

        private void CreateVehicleAt(RemotePlayer player, TechType techType, String guid, Vector3 position, Quaternion rotation)
        {
            if (techType == TechType.Cyclops)
            {
                LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(player, go, guid, position, rotation));
            }
            else
            {
                GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);

                if (techPrefab != null)
                {
                    OnVehiclePrefabLoaded(player, techPrefab, guid, position, rotation);
                }
                else
                {
                    Console.WriteLine("No prefab for tech type: " + techType);
                }
            }
        }

        private void OnVehiclePrefabLoaded(RemotePlayer player, GameObject prefab, string guid, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            // Partially copied from SubConsoleCommand.OnSubPrefabLoaded
            GameObject gameObject = Utils.SpawnPrefabAt(prefab, null, spawnPosition);
            gameObject.transform.rotation = spawnRotation;
            gameObject.SetActive(true);
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));

            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody.isKinematic = false;

            GuidHelper.SetNewGuid(gameObject, guid);

            // TODO: Implement cyclops piloting, and simulation of vehicles when they are not being piloted.
        }
    }
}
