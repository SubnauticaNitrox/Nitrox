﻿using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly PlayerManager playerManager;
        private readonly IMultiplayerSession multiplayerSession;
        private readonly Dictionary<NitroxId, VehicleModel> vehiclesById;

        public delegate void VehicleCreatedHandler(GameObject gameObject);
        public event VehicleCreatedHandler VehicleCreated;

        public Vehicles(IPacketSender packetSender, PlayerManager playerManager, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.playerManager = playerManager;
            this.multiplayerSession = multiplayerSession;
            vehiclesById = new Dictionary<NitroxId, VehicleModel>();
        }

        //We need to get TechType from parameters because CraftData can't resolve TechType.Cyclops by himself
        public VehicleModel BuildVehicleModelFrom(GameObject gameObject, TechType techType)
        {
            if (VehicleHelper.IsVehicle(techType))
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(gameObject);
                Optional<Vehicle> opVehicle = Optional.OfNullable(gameObject.GetComponent<Vehicle>());

                NitroxId constructedObjectId = NitroxEntity.GetId(gameObject);
                NitroxVector3[] hsb = VehicleHelper.GetPrimalDefaultColors();
                string name = string.Empty;
                float health = 200f;

                if (opVehicle.HasValue)
                { //Seamoth & Exosuit
                    Optional<LiveMixin> liveMixin = Optional.OfNullable(opVehicle.Value.GetComponent<LiveMixin>());

                    if (liveMixin.HasValue)
                    {
                        health = liveMixin.Value.health;
                    }

                    name = opVehicle.Value.GetName();

                    if (techType == TechType.Exosuit)
                    {   //For odd reasons the default colors aren't set yet for exosuit so we force it
                        opVehicle.Value.ReflectionCall("RegenerateRenderInfo", false, false);
                    }

                    hsb = opVehicle.Value.subName.AliveOrNull()?.GetColors().ToDto();
                }
                else if (techType == TechType.Cyclops)
                { //Cyclops
                    try
                    {
                        GameObject target = NitroxEntity.RequireObjectFrom(constructedObjectId);
                        SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                        SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                        name = subNameTarget.GetName();
                        hsb = subNameTarget.AliveOrNull()?.GetColors().ToDto();

                        Optional<LiveMixin> livemixin = Optional.OfNullable(target.GetComponent<LiveMixin>());

                        if (livemixin.HasValue)
                        {
                            health = livemixin.Value.health;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{nameof(Vehicles)}: Error while trying to spawn a cyclops. Id: {constructedObjectId}");
                    }
                }
                else
                { //Rocket
                    Optional<Rocket> oprocket = Optional.OfNullable(gameObject.GetComponent<Rocket>());

                    if (oprocket.HasValue)
                    {
                        name = oprocket.Value.subName.AliveOrNull()?.GetName();
                        hsb = oprocket.Value.subName.AliveOrNull()?.GetColors().ToDto();
                    }
                    else
                    {
                        Log.Error($"{nameof(Vehicles)}: Error while trying to spawn a rocket (Received {techType})");
                    }
                }

                return VehicleModelFactory.BuildFrom(
                    techType.ToDto(),
                    constructedObjectId,
                    gameObject.transform.position.ToDto(),
                    gameObject.transform.rotation.ToDto(),
                    childIdentifiers,
                    Optional.Empty,
                    name,
                    hsb ?? VehicleHelper.GetDefaultColors(techType), //Shouldn't be null now, but just in case
                    health
                );
            }

            Log.Error($"{nameof(Vehicles)}: Impossible to build from a non-vehicle GameObject (Received {techType})");

            return null;
        }

        public void SpawnDefaultBatteries(VehicleModel vehicleModel)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(vehicleModel.Id);
            SpawnDefaultBatteries(gameObject, VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(gameObject));
        }

        public void SpawnDefaultBatteries(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
        {
            Optional<EnergyMixin> opEnergy = Optional.OfNullable(constructedObject.GetComponent<EnergyMixin>());

            if (opEnergy.HasValue)
            {
                EnergyMixin mixin = opEnergy.Value;
                mixin.ReflectionSet("allowedToPlaySounds", false);
                mixin.SetBattery(mixin.defaultBattery, 1);
                mixin.ReflectionSet("allowedToPlaySounds", true);
            }

            foreach (InteractiveChildObjectIdentifier identifier in childIdentifiers)
            {
                Optional<GameObject> opChildGameObject = NitroxEntity.GetObjectFrom(identifier.Id);

                if (opChildGameObject.HasValue)
                {
                    Optional<EnergyMixin> opEnergyMixin = Optional.OfNullable(opChildGameObject.Value.GetComponent<EnergyMixin>());

                    if (opEnergyMixin.HasValue)
                    {
                        EnergyMixin mixin = opEnergyMixin.Value;
                        mixin.ReflectionSet("allowedToPlaySounds", false);
                        mixin.SetBattery(mixin.defaultBattery, 1);
                        mixin.ReflectionSet("allowedToPlaySounds", true);
                    }
                }
            }
        }

        public void CreateVehicle(VehicleModel vehicleModel)
        {
            AddVehicle(vehicleModel);
            CreateVehicle(vehicleModel.TechType.ToUnity(), vehicleModel.Id, vehicleModel.Position.ToUnity(), vehicleModel.Rotation.ToUnity(), vehicleModel.InteractiveChildIdentifiers, vehicleModel.DockingBayId, vehicleModel.Name, vehicleModel.HSB.ToUnity(), vehicleModel.Health);
        }

        public void CreateVehicle(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, float health)
        {
            try
            {
                if (techType == TechType.Cyclops)
                {
                    LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(techType, go, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health));
                }
                else
                {
                    GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);
                    Validate.NotNull(techPrefab, $"{nameof(Vehicles)}: No prefab for tech type: {techType}");

                    OnVehiclePrefabLoaded(techType, techPrefab, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Vehicles)}: Error while creating a vehicle. TechType: {techType} Id: {id}");
            }
        }

        public void UpdateVehiclePosition(VehicleMovementData vehicleModel, Optional<RemotePlayer> player)
        {
            Vehicle vehicle = null;
            SubRoot subRoot = null;

            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(vehicleModel.Id);

            if (opGameObject.HasValue)
            {
                GameObject gameObject = opGameObject.Value;

                Rocket rocket = gameObject.GetComponent<Rocket>();
                vehicle = gameObject.GetComponent<Vehicle>();
                subRoot = gameObject.GetComponent<SubRoot>();

                MultiplayerVehicleControl mvc = null;

                if (subRoot)
                {
                    mvc = subRoot.gameObject.EnsureComponent<MultiplayerCyclops>();
                    subRoot.GetComponent<LiveMixin>().health = vehicleModel.Health;
                }
                else if (vehicle)
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

                        if (vehicleModel is ExosuitMovementData)
                        {
                            ExosuitMovementData exoSuitMovement = (ExosuitMovementData)vehicleModel;
                            mvc.SetArmPositions(exoSuitMovement.LeftAimTarget.ToUnity(), exoSuitMovement.RightAimTarget.ToUnity());
                        }
                        else
                        {
                            Log.Error($"{nameof(Vehicles)}: Got exosuit vehicle but no ExosuitMovementData");
                        }
                    }

                    vehicle.GetComponent<LiveMixin>().health = vehicleModel.Health;
                }
                else if (rocket)
                {
                    opGameObject.Value.transform.position = vehicleModel.Position.ToUnity();
                    opGameObject.Value.transform.rotation = vehicleModel.Rotation.ToUnity();
                }

                if (mvc)
                {
                    mvc.SetPositionVelocityRotation(
                        vehicleModel.Position.ToUnity(),
                        vehicleModel.Velocity.ToUnity(),
                        vehicleModel.Rotation.ToUnity(),
                        vehicleModel.AngularVelocity.ToUnity()
                    );
                    mvc.SetThrottle(vehicleModel.AppliedThrottle);
                    mvc.SetSteeringWheel(vehicleModel.SteeringWheelYaw, vehicleModel.SteeringWheelPitch);
                }
            }

            if (player.HasValue)
            {
                RemotePlayer playerInstance = player.Value;
                playerInstance.SetVehicle(vehicle);
                playerInstance.SetSubRoot(subRoot);
                playerInstance.SetPilotingChair(subRoot?.GetComponentInChildren<PilotingChair>());
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
        }

        private void OnVehiclePrefabLoaded(TechType techType, GameObject prefab, NitroxId id, Vector3 spawnPosition, Quaternion spawnRotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, float health)
        {
            // Partially copied from SubConsoleCommand.OnSubPrefabLoaded
            GameObject gameObject = Utils.SpawnPrefabAt(prefab, null, spawnPosition);
            gameObject.transform.rotation = spawnRotation;
            gameObject.SetActive(true);
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);

            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
            Rigidbody rigidBody = gameObject.RequireComponent<Rigidbody>();
            rigidBody.isKinematic = false;

            NitroxEntity.SetNewId(gameObject, id);

            // Updates names and colours with persisted data
            if (techType == TechType.Seamoth || techType == TechType.Exosuit)
            {
                Vehicle vehicle = gameObject.GetComponent<Vehicle>();

                if (dockingBayId.HasValue)
                {
                    GameObject dockingBayBase = NitroxEntity.RequireObjectFrom(dockingBayId.Value);
                    VehicleDockingBay dockingBay = dockingBayBase.GetComponentInChildren<VehicleDockingBay>();
                    dockingBay.DockVehicle(vehicle);
                }
                else if (techType == TechType.Exosuit)
                {
                    // exosuits tend to fall through the ground after spawning. This should prevent that
                    vehicle.ReflectionSet("onGround", true);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    vehicle.vehicleName = name;
                    vehicle.subName?.DeserializeName(vehicle.vehicleName);
                }

                if (hsb != null)
                {
                    vehicle.vehicleColors = hsb;
                    vehicle.subName?.DeserializeColors(hsb);
                }

                vehicle.GetComponent<LiveMixin>().health = health;
            }
            else if (techType == TechType.Cyclops)
            {
                GameObject target = NitroxEntity.RequireObjectFrom(id);
                SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                subNameInput.OnNameChange(name);
                subNameTarget.DeserializeColors(hsb);

                target.GetComponent<LiveMixin>().health = health;

                // Set internal and external lights via runtime query to avoid circular dependencies
                NitroxServiceLocator.LocateService<Cyclops>().SetAllModes(GetVehicles<CyclopsModel>(id));
            }
            else if (techType == TechType.RocketBase)
            {
                Rocket rocket = gameObject.RequireComponent<Rocket>();

                if (!string.IsNullOrEmpty(name))
                {
                    rocket.rocketName = name;
                    rocket.subName?.DeserializeName(name);
                }

                if (hsb != null)
                {
                    rocket.rocketColors = hsb;
                    rocket.subName?.DeserializeColors(hsb);
                }
            }

            VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(gameObject, interactiveChildIdentifiers);

            // Send event after everything is created            
            if (VehicleCreated != null)
            {
                VehicleCreated(gameObject);
            }
        }

        public void DestroyVehicle(NitroxId id, bool isPiloting)
        {
            Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(id);
            if (gameObject.HasValue)
            {
                Vehicle vehicle = gameObject.Value.RequireComponent<Vehicle>();

                if (isPiloting) //Check Remote Object Have Player inside
                {
                    ushort pilot = ushort.Parse(vehicle.pilotId);
                    Optional<RemotePlayer> remotePilot = playerManager.Find(pilot);

                    if (remotePilot.HasValue) // Get Remote Player Inside == vehicle.pilotId  Remove From Vehicle Before Destroy
                    {
                        RemotePlayer remotePlayer = remotePilot.Value;
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

                //Destroy vehicle
                if (vehicle.gameObject)
                {
                    if (vehicle.destructionEffect)
                    {
                        GameObject vehicleGameObject = Object.Instantiate(vehicle.destructionEffect);
                        vehicleGameObject.transform.position = vehicle.transform.position;
                        vehicleGameObject.transform.rotation = vehicle.transform.rotation;
                    }

                    Object.Destroy(vehicle.gameObject);
                    RemoveVehicle(id);
                }
            }
        }

        public void UpdateVehicleChildren(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildrenGuids)
        {
            Optional<GameObject> go = NitroxEntity.GetObjectFrom(id);
            Validate.IsTrue(go.HasValue, $"Tried to set children ids for vehicle but could not find it with Nitrox id '{id}'");

            VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(go.Value, interactiveChildrenGuids);
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
                NitroxId id = NitroxEntity.GetId(vehicle.gameObject);
                LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

                VehicleDestroyed vehicleDestroyed = new VehicleDestroyed(id, localPlayer.PlayerName, vehicle.GetPilotingMode());
                packetSender.Send(vehicleDestroyed);

                // If there is a pilotId then there is a remote player.  We must
                // detach the remote player before destroying the game object.
                if (!string.IsNullOrEmpty(vehicle.pilotId))
                {
                    ushort pilot = ushort.Parse(vehicle.pilotId);
                    Optional<RemotePlayer> remotePilot = playerManager.Find(pilot);

                    if (remotePilot.HasValue)
                    {
                        RemotePlayer remotePlayer = remotePilot.Value;
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
                dockId = NitroxEntity.GetId(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else if (dockingBay.GetSubRoot() is SubRoot)
            {
                dockId = NitroxEntity.GetId(dockingBay.GetSubRoot().gameObject);
            }
            else
            {
                dockId = NitroxEntity.GetId(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
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
                dockId = NitroxEntity.GetId(dockingBay.GetComponentInParent<BaseRoot>().gameObject);
            }
            else if (dockingBay.GetSubRoot() is SubRoot)
            {
                dockId = NitroxEntity.GetId(dockingBay.GetSubRoot().gameObject);
            }
            else
            {
                dockId = NitroxEntity.GetId(dockingBay.GetComponentInParent<ConstructableBase>().gameObject);
            }

            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleUndocking packet = new VehicleUndocking(vehicleId, dockId, playerId);
            packetSender.Send(packet);
        }

        /*
         A poorly timed movement packet will cause major problems when docking because the remote 
         player will think that the player is no longer in a vehicle.  Unfortunately, the game calls
         the vehicle exit code before the animation completes so we need to suppress any side affects.
         Two thing we want to protect against:

             1) If a movement packet is received when docking, the player might exit the vehicle early
                and it will show them sitting outside the vehicle during the docking animation.

             2) If a movement packet is received when undocking, the player game object will be stuck in
                place until after the player exits the vehicle.  This causes the player body to stretch to
                the current cyclops position.
        */
        public IEnumerator AllowMovementPacketsAfterDockingAnimation(PacketSuppressor<Movement> movementSuppressor)
        {
            yield return new WaitForSeconds(3.0f);
            movementSuppressor.Dispose();
        }

        public IEnumerator UpdateVehiclePositionAfterSpawn(VehicleModel vehicleModel, GameObject gameObject, float cooldown)
        {
            yield return new WaitForSeconds(cooldown);

            VehicleMovementData vehicleMovementData = new VehicleMovementData(vehicleModel.TechType, vehicleModel.Id, gameObject.transform.position.ToDto(), gameObject.transform.rotation.ToDto(), vehicleModel.Health);
            ushort playerId = ushort.MaxValue;

            packetSender.Send(new VehicleMovement(playerId, vehicleMovementData));
        }

        public void BroadcastOnPilotModeChanged(Vehicle vehicle, bool isPiloting)
        {
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleOnPilotModeChanged packet = new VehicleOnPilotModeChanged(NitroxEntity.GetId(vehicle.gameObject), playerId, isPiloting);
            packetSender.Send(packet);
        }

        public void SetOnPilotMode(NitroxId vehicleId, ushort playerId, bool isPiloting)
        {
            Optional<GameObject> opVehicle = NitroxEntity.GetObjectFrom(vehicleId);
            if (!opVehicle.HasValue)
            {
                return;
            }

            GameObject gameObject = opVehicle.Value;
            Vehicle vehicle = gameObject.GetComponent<Vehicle>();
            if (!vehicle)
            {
                return;
            }

            vehicle.pilotId = isPiloting ? playerId.ToString() : string.Empty;
        }

        public void AddVehicle(VehicleModel vehicleModel)
        {
            if (!vehiclesById.ContainsKey(vehicleModel.Id))
            {
                vehiclesById.Add(vehicleModel.Id, vehicleModel);
                Log.Debug($"{nameof(Vehicles)}: Added vehicle {vehicleModel}");
            }
            else
            {
                Log.Error($"{nameof(Vehicles)}: Error while adding an existing vehicle id {vehicleModel.Id}");
            }
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

        public Optional<T> TryGetVehicle<T>(NitroxId vehicleId) where T : VehicleModel
        {
            vehiclesById.TryGetValue(vehicleId, out VehicleModel vehicle);
            return Optional.OfNullable((T)vehicle);
        }
    }
}
