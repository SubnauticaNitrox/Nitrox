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

        public Vehicles(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void AddVehiclePosition(VehicleModel vehicleModel)
        {
            CreateVehicleAt(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation);
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

        private void CreateVehicleAt(TechType techType, string guid, Vector3 position, Quaternion rotation)
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

        public void DestroyVehicle(VehicleDestroyed entry, PlayerManager remotePlayerManager) //Destroy Vehicle From network
        {
            Optional<GameObject> Object = GuidHelper.GetObjectFrom(entry.Guid);
            if (Object.IsPresent())
            {
                GameObject T = Object.Get();
                Vehicle vehicle = T.RequireComponent<Vehicle>();

                if (entry.GetPilotingMode) //Check Remote Object Have Player inside
                {
                    Optional<RemotePlayer> player = remotePlayerManager.Find(vehicle.pilotId);

                    if (player.IsPresent()) // Get Remote Player Inside == vehicle.pilotId  Remove From Vehicle Before Destroy
                    {
                        RemotePlayer playerInstance = player.Get();
                        playerInstance.SetVehicle(null);
                        playerInstance.SetSubRoot(null);
                        playerInstance.SetPilotingChair(null);
                        playerInstance.AnimationController.UpdatePlayerAnimations = true;
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

        public void CreateNewVehicle(Vehicle vehicle) //Add Vehicle Fron Network Packet
        {
            string guid = GuidHelper.GetGuid(vehicle.gameObject);
            Vector3 position = vehicle.gameObject.transform.position;
            Quaternion rotation = vehicle.gameObject.transform.rotation;
            TechType techType = CraftData.GetTechType(vehicle.gameObject);
            VehicleModel model = new VehicleModel(techType,guid,position,rotation,new Vector3(), new Vector3(), 0,0,false);
            VehicleCreated vehicleAdd = new VehicleCreated(model);
            packetSender.Send(vehicleAdd);
        }

        public void Add(VehicleModel vehicleModel) //Add Vehicle From Save
        {
            CreateVehicleAt(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation);
        }

        public void Remove(Vehicle vehicle) //Remove Vehicle From Instance Event OnKill
        {
            using (packetSender.Suppress<VehicleOnPilotModeChanged>())
            {
                string guid = GuidHelper.GetGuid(vehicle.gameObject);
                LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
                VehicleDestroyed vehicleremove = new VehicleDestroyed(guid, localPlayer.PlayerName, vehicle.GetPilotingMode());
                packetSender.Send(vehicleremove);

                GuidHelper.SetNewGuid(vehicle.gameObject, string.Empty); //Clean Local (Detach Player From Vehicle Call OnPilotMode Event if Guid is Empty Dont Send That Event)

                if (!string.IsNullOrEmpty(vehicle.pilotId)) // OnPilotMode Set That Value if Value exist have Remote Player Inside (Need Detach Before Destroy gameObject)
                {
                    PlayerManager remotePlayer = NitroxServiceLocator.LocateService<PlayerManager>();
                    Optional<RemotePlayer> player = remotePlayer.Find(vehicle.pilotId);
                    if (player.IsPresent())
                    {
                        RemotePlayer RP = player.Get();
                        RP.SetVehicle(null);
                        RP.SetSubRoot(null);
                        RP.SetPilotingChair(null);
                        RP.AnimationController.UpdatePlayerAnimations = true;
                    }
                }
            }
        }

        public void OnPilotMode(Vehicle vehicle, bool type) // Set & Unset Remote Player Value Inside of Vehicle Sender
        {
            if (!string.IsNullOrEmpty(vehicle.gameObject.GetGuid()))
            {
                VehicleOnPilotModeChanged mode = new VehicleOnPilotModeChanged(vehicle.gameObject.GetGuid(), GuidHelper.GetGuid(Player.main.gameObject), type);
                packetSender.Send(mode);
            }
            
        }

        public void OnPilotModeSet(string vehicleGuid, string playerGuid, bool type) // Set & Unset Remote Player Value Inside of Vehicle Receiver
        {
            Log.Info("Receiver: VehicleGuid: " + vehicleGuid + " PlayerGuid: " + playerGuid + " Type: " + type);

            Optional<GameObject> Vehicle = GuidHelper.GetObjectFrom(vehicleGuid);

            if (Vehicle.IsPresent())
            {
                GameObject Go = Vehicle.Get();
                Vehicle V = Go.GetComponent<Vehicle>();

                if (V != null)
                {
                    if (type)
                    {
                        V.pilotId = playerGuid;
                    }
                    else
                    {
                        V.pilotId = string.Empty;
                    }
                }
            }
        }
    }
}
