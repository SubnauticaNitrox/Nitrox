using System;
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
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private Cyclops cyclops;
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
            cyclops = null;
        }

        //We need to get TechType from parameters because CraftData can't resolve TechType.Cyclops by himself
        public VehicleModel BuildVehicleModelFrom(GameObject gameObject, TechType techType)
        {
            if (VehicleHelper.IsVehicle(techType))
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(gameObject);
                Optional<Vehicle> opvehicle = Optional.OfNullable(gameObject.GetComponent<Vehicle>());

                NitroxId constructedObjectId = NitroxEntity.GetId(gameObject);
                Vector3[] HSB = VehicleHelper.GetPrimalDefaultColours();
                string name = string.Empty;
                float health = 200f;


                if (opvehicle.HasValue)
                { //Seamoth & Exosuit
                    Optional<LiveMixin> livemixin = Optional.OfNullable(opvehicle.Value.GetComponent<LiveMixin>());

                    if (livemixin.HasValue)
                    {
                        health = livemixin.Value.health;
                    }

                    name = opvehicle.Value.GetName();

                    if (techType == TechType.Exosuit)
                    {   //For odd reasons the default colors aren't set yet for exosuit so we force it
                        opvehicle.Value.ReflectionCall("RegenerateRenderInfo", false, false);
                    }

                    HSB = opvehicle.Value.subName?.GetColors();
                }
                else
                { //Cyclops
                    try
                    {
                        GameObject target = NitroxEntity.RequireObjectFrom(constructedObjectId);
                        SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
                        SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

                        name = subNameTarget?.GetName();
                        HSB = subNameTarget?.GetColors();

                        Optional<LiveMixin> livemixin = Optional.OfNullable(target.GetComponent<LiveMixin>());

                        if (livemixin.HasValue)
                        {
                            health = livemixin.Value.health;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{nameof(Vehicles)}: Error while trying to spawn a cyclops ({constructedObjectId})", ex);
                    }
                }

                return VehicleModelFactory.BuildFrom(
                    techType.Model(),
                    constructedObjectId,
                    gameObject.transform.position,
                    gameObject.transform.rotation,
                    childIdentifiers,
                    Optional.Empty,
                    name,
                    HSB ?? VehicleHelper.GetDefaultColours(techType), //Shouldn't be null now, but just in case
                    health
                );
            }
            else
            {
                Log.Error($"{nameof(Vehicles)}: Impossible to build from a non-vehicle GameObject (Received {techType})");
            }

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
            CreateVehicle(vehicleModel.TechType.Enum(), vehicleModel.Id, vehicleModel.Position, vehicleModel.Rotation, vehicleModel.InteractiveChildIdentifiers, vehicleModel.DockingBayId, vehicleModel.Name, vehicleModel.HSB, vehicleModel.Health);
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
                Log.Error($"{nameof(Vehicles)}: Error while creating a vehicle {techType} {id}", ex);
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

            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(vehicleModel.Id);

            if (opGameObject.HasValue)
            {
                GameObject gameObject = opGameObject.Value;

                vehicle = gameObject.GetComponent<Vehicle>();
                subRoot = gameObject.GetComponent<SubRoot>();

                MultiplayerVehicleControl mvc = null;

                if (subRoot != null)
                {
                    mvc = subRoot.gameObject.EnsureComponent<MultiplayerCyclops>();
                    subRoot.GetComponent<LiveMixin>().health = vehicleModel.Health;
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
                        }
                        else
                        {
                            Log.Error($"{nameof(Vehicles)}: Got exosuit vehicle but no ExosuitMovementData");
                        }
                    }

                    vehicle.GetComponent<LiveMixin>().health = vehicleModel.Health;
                }

                if (mvc != null)
                {
                    mvc.SetPositionVelocityRotation(remotePosition, remoteVelocity, remoteRotation, angularVelocity);
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

                // Set internal and external lights
                SetCyclopsModes(id);
            }

            VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(gameObject, interactiveChildIdentifiers);

            // Send event after everything is created            
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

        public void DestroyVehicle(NitroxId id, bool isPiloting)
        {
            Optional<GameObject> Object = NitroxEntity.GetObjectFrom(id);
            if (Object.HasValue)
            {
                GameObject go = Object.Value;
                Vehicle vehicle = go.RequireComponent<Vehicle>();

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
                if (vehicle.gameObject != null)
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
            if (vehicle == null)
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
            VehicleModel vehicle;
            vehiclesById.TryGetValue(vehicleId, out vehicle);
            return Optional.OfNullable((T)vehicle);
        }
    }
}
