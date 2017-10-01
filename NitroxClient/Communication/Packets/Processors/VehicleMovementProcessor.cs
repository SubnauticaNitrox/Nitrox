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
        private readonly PlayerManager remotePlayerManager;

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
            Vector3 angularVelocity = vehicleMovement.AngularVelocity;

            Vehicle vehicle = null;
            SubRoot subRoot = null;
            if (opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();

                vehicle = gameObject.GetComponent<Vehicle>();
                subRoot = gameObject.GetComponent<SubRoot>();

                MultiplayerVehicleControl mvc = null;

                if (subRoot != null)
                {
                    mvc = subRoot.gameObject.EnsureComponent<MultiplayerCyclops>();
                }
                else if (vehicle != null)
                {
                    var seamoth = vehicle as SeaMoth;
                    var exosuit = vehicle as Exosuit;

                    if (seamoth)
                    {
                        mvc = seamoth.gameObject.EnsureComponent<MultiplayerSeaMoth>();
                    }
                    else if (exosuit)
                    {
                        mvc = exosuit.gameObject.EnsureComponent<MultiplayerExosuit>();
                    }
                }

                if (mvc != null)
                {
                    mvc.SetPositionVelocityRotation(remotePosition, remoteVelocity, remoteRotation, angularVelocity);
                    mvc.SetThrottle(vehicleMovement.AppliedThrottle);
                    mvc.SetSteeringWheel(vehicleMovement.SteeringWheelYaw, vehicleMovement.SteeringWheelPitch);
                }
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
        }
    }
}
