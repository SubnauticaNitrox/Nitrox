﻿using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly PlayerManager playerManager;

        public Vehicles(IPacketSender packetSender, PlayerManager playerManager)
        {
            this.packetSender = packetSender;
            this.playerManager = playerManager;
        }

        public void CreateVehicle(VehicleModel vehicleModel)
        {
            CreateVehicle(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation);
        }

        public void CreateVehicle(TechType techType, string guid, Vector3 position, Quaternion rotation)
        {
            if (techType == TechType.Cyclops)
            {
                LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(go, guid, position, rotation));
            }
            else
            {
                GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);

                if (techPrefab != null)
                {
                    OnVehiclePrefabLoaded(techPrefab, guid, position, rotation);
                }
                else
                {
                    Log.Error("No prefab for tech type: " + techType);
                }
            }
        }

        public void UpdateVehiclePosition(VehicleModel vehicleModel, Optional<RemotePlayer> player)
        {
            Vector3 remotePosition = vehicleModel.Position;
            Vector3 remoteVelocity = vehicleModel.Velocity;
            Quaternion remoteRotation = vehicleModel.Rotation;
            Vector3 angularVelocity = vehicleModel.AngularVelocity;

            Vehicle vehicle = null;
            SubRoot subRoot = null;

            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicleModel.Guid);

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
                    SeaMoth seamoth = vehicle as SeaMoth;
                    Exosuit exosuit = vehicle as Exosuit;

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
                    mvc.SetThrottle(vehicleModel.AppliedThrottle);
                    mvc.SetSteeringWheel(vehicleModel.SteeringWheelYaw, vehicleModel.SteeringWheelPitch);                    
                }
            }

            if (player.IsPresent())
            {
                RemotePlayer playerInstance = player.Get();
                playerInstance.SetVehicle(vehicle);
                playerInstance.SetSubRoot(subRoot);
                playerInstance.SetPilotingChair(subRoot?.GetComponentInChildren<PilotingChair>());
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
        }

        private void OnVehiclePrefabLoaded(GameObject prefab, string guid, Vector3 spawnPosition, Quaternion spawnRotation)
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

        public void DestroyVehicle(string guid, bool isPiloting) //Destroy Vehicle From network
        {
            Optional<GameObject> Object = GuidHelper.GetObjectFrom(guid);
            if (Object.IsPresent())
            {
                GameObject T = Object.Get();
                Vehicle vehicle = T.RequireComponent<Vehicle>();

                if (isPiloting) //Check Remote Object Have Player inside
                {
                    Optional<RemotePlayer> remotePilot = playerManager.Find(vehicle.pilotId);

                    if (remotePilot.IsPresent()) // Get Remote Player Inside == vehicle.pilotId  Remove From Vehicle Before Destroy
                    {
                        RemotePlayer remotePlayer = remotePilot.Get();
                        remotePlayer.SetVehicle(null);
                        remotePlayer.SetSubRoot(null);
                        remotePlayer.SetPilotingChair(null);
                        remotePlayer.AnimationController.UpdatePlayerAnimations = true;
                    }
                }

                if (vehicle.GetPilotingMode()) //Check Local Object Have Player inside
                {
                    vehicle.ReflectionCall("OnPilotModeEnd", false, false, null);

                    if (!Player.main.ToNormalMode(true))
                    {
                        Player.main.ToNormalMode(false);
                        Player.main.transform.parent = null;
                    }
                }

                if (vehicle.gameObject != null) // Destroy Vehicle
                {
                    if (vehicle.destructionEffect)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(vehicle.destructionEffect);
                        gameObject.transform.position = vehicle.transform.position;
                        gameObject.transform.rotation = vehicle.transform.rotation;
                    }

                    UnityEngine.Object.Destroy(vehicle.gameObject);
                }
            }
        }
        
        public void BroadcastDestroyedVehicle(Vehicle vehicle)
        {
            using (packetSender.Suppress<VehicleOnPilotModeChanged>())
            {
                string guid = GuidHelper.GetGuid(vehicle.gameObject);
                LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

                VehicleDestroyed vehicleDestroyed = new VehicleDestroyed(guid, localPlayer.PlayerName, vehicle.GetPilotingMode());
                packetSender.Send(vehicleDestroyed);

                // Remove vehicle guid (Detach Player From Vehicle Call OnPilotMode Event if Guid is Empty Dont Send That Event)
                GuidHelper.SetNewGuid(vehicle.gameObject, string.Empty); 

                // If there is a pilotId then there is a remote player.  We must
                // detach the remote player before destroying the game object.
                if (!string.IsNullOrEmpty(vehicle.pilotId)) 
                {
                    Optional<RemotePlayer> pilot = playerManager.Find(vehicle.pilotId);

                    if (pilot.IsPresent())
                    {
                        RemotePlayer remotePlayer = pilot.Get();
                        remotePlayer.SetVehicle(null);
                        remotePlayer.SetSubRoot(null);
                        remotePlayer.SetPilotingChair(null);
                        remotePlayer.AnimationController.UpdatePlayerAnimations = true;
                    }
                }
            }
        }

        public void BroadcastNewVehicle(Vehicle vehicle)
        {
            string guid = GuidHelper.GetGuid(vehicle.gameObject);
            Vector3 position = vehicle.gameObject.transform.position;
            Quaternion rotation = vehicle.gameObject.transform.rotation;
            TechType techType = CraftData.GetTechType(vehicle.gameObject);

            VehicleModel model = new VehicleModel(techType, guid, position, rotation, new Vector3(), new Vector3(), 0, 0, false);

            VehicleCreated vehicleCreated = new VehicleCreated(model);
            packetSender.Send(vehicleCreated);
        }

        public void BroadcastOnPilotModeChanged(Vehicle vehicle, bool isPiloting)
        {
            if (!string.IsNullOrEmpty(vehicle.gameObject.GetGuid()))
            {
                VehicleOnPilotModeChanged packet = new VehicleOnPilotModeChanged(vehicle.gameObject.GetGuid(), GuidHelper.GetGuid(Player.main.gameObject), isPiloting);
                packetSender.Send(packet);
            }            
        }

        public void SetOnPilotMode(string vehicleGuid, string playerGuid, bool isPiloting)
        {
            Optional<GameObject> opVehicle = GuidHelper.GetObjectFrom(vehicleGuid);

            if (opVehicle.IsPresent())
            {
                GameObject gameObject = opVehicle.Get();
                Vehicle vehicle = gameObject.GetComponent<Vehicle>();

                if (vehicle != null)
                {
                    if (isPiloting)
                    {
                        vehicle.pilotId = playerGuid;
                    }
                    else
                    {
                        vehicle.pilotId = string.Empty;
                    }
                }
            }
        }
    }
}
