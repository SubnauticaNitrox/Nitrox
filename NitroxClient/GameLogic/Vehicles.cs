using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
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
using NitroxClient.Communication;
using System.Collections;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly PlayerManager playerManager;
        private readonly IMultiplayerSession multiplayerSession;

        public Vehicles(IPacketSender packetSender, PlayerManager playerManager, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.playerManager = playerManager;
            this.multiplayerSession = multiplayerSession;
        }

        public void CreateVehicle(VehicleModel vehicleModel)
        {
            CreateVehicle(vehicleModel.TechType, vehicleModel.Guid, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, vehicleModel.DockingBayGuid);
        }

        public void CreateVehicle(TechType techType, string guid, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid)
        {
            if (techType == TechType.Cyclops)
            {
                LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(go, guid, position, rotation, interactiveChildIdentifiers, dockingBayGuid));
            }
            else
            {
                GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);

                if (techPrefab != null)
                {
                    OnVehiclePrefabLoaded(techPrefab, guid, position, rotation, interactiveChildIdentifiers, dockingBayGuid);
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

        private void OnVehiclePrefabLoaded(GameObject prefab, string guid, Vector3 spawnPosition, Quaternion spawnRotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid)
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
            if (interactiveChildIdentifiers.IsPresent())
            {
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenGuids(gameObject, interactiveChildIdentifiers.Get()); //Copy From ConstructorBeginCraftingProcessor
            }

            if(dockingBayGuid.IsPresent())
            {
                GameObject dockingBayBase = GuidHelper.RequireObjectFrom(dockingBayGuid.Get());
                VehicleDockingBay dockingBay = dockingBayBase.GetComponentInChildren<VehicleDockingBay>();

                Vehicle vehicle = gameObject.GetComponent<Vehicle>();

                dockingBay.DockVehicle(vehicle);
            }
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
                }
            }
        }

        public void UpdateVehicleChildren(string guid, List<InteractiveChildObjectIdentifier> interactiveChildrenGuids)
        {
            Optional<GameObject> Object = GuidHelper.GetObjectFrom(guid);
            if (Object.IsPresent())
            {
                GameObject T = Object.Get();
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenGuids(T, interactiveChildrenGuids);
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
            string dockGuid = string.Empty;

            if (dockingBay.GetSubRoot() is BaseRoot)
            {
                dockGuid = GuidHelper.GetGuid(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else
            {
                dockGuid = GuidHelper.GetGuid(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            string vehicleGuid = GuidHelper.GetGuid(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleDocking packet = new VehicleDocking(vehicleGuid, dockGuid, playerId);
            packetSender.Send(packet);

            PacketSuppressor<Movement> movementSuppressor = packetSender.Suppress<Movement>();
            vehicle.StartCoroutine(AllowMovementPacketsAfterDockingAnimation(movementSuppressor));
        }

        public void BroadcastVehicleUndocking(VehicleDockingBay dockingBay, Vehicle vehicle)
        {
            string dockGuid = string.Empty;

            if (dockingBay.GetSubRoot() is BaseRoot)
            {
                dockGuid = GuidHelper.GetGuid(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else
            {
                dockGuid = GuidHelper.GetGuid(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            string vehicleGuid = GuidHelper.GetGuid(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleUndocking packet = new VehicleUndocking(vehicleGuid, dockGuid, playerId);
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
            if (!string.IsNullOrEmpty(vehicle.gameObject.GetGuid()))
            {
                ushort playerId = multiplayerSession.Reservation.PlayerId;

                VehicleOnPilotModeChanged packet = new VehicleOnPilotModeChanged(vehicle.gameObject.GetGuid(), playerId, isPiloting);
                packetSender.Send(packet);
            }
        }

        public void SetOnPilotMode(string vehicleGuid, ushort playerId, bool isPiloting)
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
                        vehicle.pilotId = playerId.ToString();
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
