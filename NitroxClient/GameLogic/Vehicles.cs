﻿using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly PlayerManager playerManager;
        private readonly IMultiplayerSession multiplayerSession;
        private Cyclops cyclops;
        private readonly Dictionary<NitroxId, VehicleModel> vehiclesById = new Dictionary<NitroxId, VehicleModel>();
        public delegate void VehicleCreatedHandler(GameObject gameObject);
        public event VehicleCreatedHandler VehicleCreated;

        public Vehicles(IPacketSender packetSender, PlayerManager playerManager, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.playerManager = playerManager;
            this.multiplayerSession = multiplayerSession;
            cyclops = null;
        }

        public void CreateVehicle(VehicleModel vehicleModel)
        {
            AddVehicle(vehicleModel);
            CreateVehicle(vehicleModel.TechType.Enum(), vehicleModel.Id, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, vehicleModel.DockingBayId, vehicleModel.Name, vehicleModel.HSB, vehicleModel.Colours);            
        }

        public void CreateVehicle(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, Vector3[] colours)
        {
            if (techType == TechType.Cyclops)
            {
                LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(techType, go, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, colours));
            }
            else
            {
                GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);

                if (techPrefab != null)
                {
                    OnVehiclePrefabLoaded(techType, techPrefab, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, colours);
                }
                else
                {
                    Log.Error("No prefab for tech type: " + techType);
                }
            }
        }

        public void UpdateVehiclePosition(VehicleMovementData vehicleModel, Optional<RemotePlayer> player)
        {
            Vector3 remotePosition = vehicleModel.Position;
            Vector3 remoteVelocity = vehicleModel.Velocity;
            Quaternion remoteRotation = vehicleModel.Rotation;
            Vector3 angularVelocity = vehicleModel.AngularVelocity;

            Vehicle vehicle = null;
            SubRoot subRoot = null;

            Optional<GameObject> opGameObject = NitroxIdentifier.GetObjectFrom(vehicleModel.Id);

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
                        if (vehicleModel.GetType() == typeof(ExosuitMovementData))
                        {
                            ExosuitMovementData exoSuitMovement = (ExosuitMovementData)vehicleModel;
                            mvc.SetArmPositions(exoSuitMovement.LeftAimTarget, exoSuitMovement.RightAimTarget);
                        } else
                        {
                            Log.Error("Got exosuit vehicle but no ExosuitMovementData");
                        }
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

        private void OnVehiclePrefabLoaded(TechType techType, GameObject prefab, NitroxId id, Vector3 spawnPosition, Quaternion spawnRotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, Vector3[] colours)
        {
            // Partially copied from SubConsoleCommand.OnSubPrefabLoaded
            GameObject gameObject = Utils.SpawnPrefabAt(prefab, null, spawnPosition);
            gameObject.transform.rotation = spawnRotation;
            gameObject.SetActive(true);
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody.isKinematic = false;
            NitroxIdentifier.SetNewId(gameObject, id);
            // Updates names and colours with persisted data
            if (techType == TechType.Seamoth || techType == TechType.Exosuit)
            {
                Vehicle vehicle = gameObject.GetComponent<Vehicle>();
                if (dockingBayId.IsPresent())
                {
                    GameObject dockingBayBase = NitroxIdentifier.RequireObjectFrom(dockingBayId.Get());
                    VehicleDockingBay dockingBay = dockingBayBase.GetComponentInChildren<VehicleDockingBay>();
                    dockingBay.DockVehicle(vehicle);
                } else if(techType == TechType.Exosuit)
                {
                    // exosuits tend to fall through the ground after spawning. This should prevent that
                    vehicle.ReflectionSet("onGround", true);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    vehicle.vehicleName = name;
                    vehicle.subName.DeserializeName(vehicle.vehicleName);
                }

                if (colours != null)
                {
                    Vector3[] colour = new Vector3[5];

                    for (int i = 0; i < hsb.Length; i++)
                    {
                        colour[i] = hsb[i];
                    }
                    vehicle.vehicleColors = colour;
                    vehicle.subName.DeserializeColors(vehicle.vehicleColors);
                }
            }
            else if(techType == TechType.Cyclops)
            {
                GameObject target = NitroxIdentifier.RequireObjectFrom(id);
                SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");
                subNameInput.OnNameChange(name);
                for (int i = 0; i < hsb.Length; i++)
                {
                    subNameInput.SetSelected(i);
                    Color tmpColour = new Vector4(colours[i].x, colours[i].y, colours[i].z);
                    subNameTarget.SetColor(i, hsb[i], tmpColour);
                    subNameTarget.DeserializeColors(hsb);
                }

                // Set internal and external lights
                SetCyclopsModes(id);
            }
            if (interactiveChildIdentifiers.IsPresent())
            {
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(gameObject, interactiveChildIdentifiers.Get()); //Copy From ConstructorBeginCraftingProcessor
            }            

            // Send event after everthing is created            
            if (VehicleCreated != null)
            {
                VehicleCreated(gameObject);
            }
        }

        private void SetCyclopsModes(NitroxId id)
        {
            if (cyclops == null)
            {
                cyclops = NitroxServiceLocator.LocateService<Cyclops>();
            }
            cyclops.SetAllModes(GetVehicles<CyclopsModel>(id));
        }

        public void DestroyVehicle(NitroxId id, bool isPiloting) //Destroy Vehicle From network
        {
            Optional<GameObject> Object = NitroxIdentifier.GetObjectFrom(id);
            if (Object.IsPresent())
            {
                GameObject T = Object.Get();
                Vehicle vehicle = T.RequireComponent<Vehicle>();

                if (isPiloting) //Check Remote Object Have Player inside
                {
                    ushort pilot = ushort.Parse(vehicle.pilotId);
                    Optional<RemotePlayer> remotePilot = playerManager.Find(pilot);

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
                    RemoveVehicle(id);
                }
            }
        }

        public void UpdateVehicleChildren(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildrenGuids)
        {
            Optional<GameObject> Object = NitroxIdentifier.GetObjectFrom(id);
            if (Object.IsPresent())
            {
                GameObject T = Object.Get();
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(T, interactiveChildrenGuids);
            }
        }

        public void BroadcastCreatedVehicle(VehicleModel vehicle)
        {
            LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

            VehicleCreated vehicleCreated = new VehicleCreated(vehicle, localPlayer.PlayerName);
            packetSender.Send(vehicleCreated);
        }

        public void BroadcastDestroyedVehicle(Vehicle vehicle)
        {
            using (packetSender.Suppress<VehicleOnPilotModeChanged>())
            {
                NitroxId id = NitroxIdentifier.GetId(vehicle.gameObject);
                LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

                VehicleDestroyed vehicleDestroyed = new VehicleDestroyed(id, localPlayer.PlayerName, vehicle.GetPilotingMode());
                packetSender.Send(vehicleDestroyed);
                
                // If there is a pilotId then there is a remote player.  We must
                // detach the remote player before destroying the game object.
                if (!string.IsNullOrEmpty(vehicle.pilotId)) 
                {
                    ushort pilot = ushort.Parse(vehicle.pilotId);
                    Optional<RemotePlayer> remotePilot = playerManager.Find(pilot);

                    if (remotePilot.IsPresent())
                    {
                        RemotePlayer remotePlayer = remotePilot.Get();
                        remotePlayer.SetVehicle(null);
                        remotePlayer.SetSubRoot(null);
                        remotePlayer.SetPilotingChair(null);
                        remotePlayer.AnimationController.UpdatePlayerAnimations = true;
                    }
                }
            }
        }

        public void BroadcastVehicleDocking(VehicleDockingBay dockingBay, Vehicle vehicle)
        {
            NitroxId dockId;

            if (dockingBay.GetSubRoot() is BaseRoot)
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else if (dockingBay.GetSubRoot() is SubRoot)
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetSubRoot().gameObject);
            }
            else
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            NitroxId vehicleId = NitroxIdentifier.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleDocking packet = new VehicleDocking(vehicleId, dockId, playerId);
            packetSender.Send(packet);

            PacketSuppressor<Movement> movementSuppressor = packetSender.Suppress<Movement>();
            vehicle.StartCoroutine(AllowMovementPacketsAfterDockingAnimation(movementSuppressor));
        }

        public void BroadcastVehicleUndocking(VehicleDockingBay dockingBay, Vehicle vehicle)
        {
            NitroxId dockId;

            if (dockingBay.GetSubRoot() is BaseRoot)
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else if (dockingBay.GetSubRoot() is SubRoot)
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetSubRoot().gameObject);
            }
            else
            {
                dockId = NitroxIdentifier.GetId(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            NitroxId vehicleId = NitroxIdentifier.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleUndocking packet = new VehicleUndocking(vehicleId, dockId, playerId);
            packetSender.Send(packet);
        }

        /*
         A poorly timed movement packet will cause major problems when docking because the remote 
         player will think that the player is no longer in a vehicle.  Unfortunetly, the game calls
         the vehicle exit code before the animation completes so we need to suppress any side affects.
         Two thing we want to protect against:
        
             1) If a movement packet is received when docking, the player might exit the vehicle early
                and it will show them sitting outside the vehicle during the docking animation.
         
             2) If a movement packet is received when undocking, the player game object will be stuck in
                place until after the player exits the vehicle.  This causes the player body to strech to
                the current cyclops position.
        */
        IEnumerator AllowMovementPacketsAfterDockingAnimation(PacketSuppressor<Movement> movementSuppressor)
        {
            yield return new WaitForSeconds(3.0f);
            movementSuppressor.Dispose();
        }

        public void BroadcastOnPilotModeChanged(Vehicle vehicle, bool isPiloting)
        {
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleOnPilotModeChanged packet = new VehicleOnPilotModeChanged(NitroxIdentifier.GetId(vehicle.gameObject), playerId, isPiloting);
            packetSender.Send(packet);
        }

        public void SetOnPilotMode(NitroxId vehicleId, ushort playerId, bool isPiloting)
        {
            Optional<GameObject> opVehicle = NitroxIdentifier.GetObjectFrom(vehicleId);

            if (opVehicle.IsPresent())
            {
                GameObject gameObject = opVehicle.Get();
                Vehicle vehicle = gameObject.GetComponent<Vehicle>();

                if (vehicle != null)
                {
                    if (isPiloting)
                    {
                        vehicle.pilotId = playerId.ToString();
                    }
                    else
                    {
                        vehicle.pilotId = string.Empty;
                    }
                }
            }
        }

        public void AddVehicle(VehicleModel vehicleModel)
        {
            vehiclesById.Add(vehicleModel.Id, vehicleModel);
        }

        public bool RemoveVehicle(VehicleModel vehicleModel)
        {
            return RemoveVehicle(vehicleModel.Id);
        }
        
        public bool RemoveVehicle(NitroxId id)
        {
            return vehiclesById.Remove(id);
        } 
        
        public T GetVehicles<T>(NitroxId vehicleId) where T : VehicleModel
        {            
            return (T)vehiclesById[vehicleId];
        }
    }
}
