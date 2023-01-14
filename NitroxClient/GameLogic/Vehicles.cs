using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.GameLogic.PlayerLogic;

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly IMultiplayerSession multiplayerSession;
        private readonly Dictionary<NitroxId, VehicleModel> vehiclesById;

        public Vehicles(IPacketSender packetSender, IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnership)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
            vehiclesById = new Dictionary<NitroxId, VehicleModel>();
        }

        public void UpdateVehiclePosition(VehicleMovementData vehicleModel, Optional<RemotePlayer> player)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(vehicleModel.Id);
            Vehicle vehicle = null;
            SubRoot subRoot = null;

            if (opGameObject.HasValue)
            {
                Rocket rocket = opGameObject.Value.GetComponent<Rocket>();
                vehicle = opGameObject.Value.GetComponent<Vehicle>();
                subRoot = opGameObject.Value.GetComponent<SubRoot>();

                MultiplayerVehicleControl mvc = null;

                if (subRoot)
                {
                    mvc = subRoot.gameObject.EnsureComponent<MultiplayerCyclops>();
                }
                else if (vehicle)
                {
                    if (vehicle.docked)
                    {
                        Log.Debug($"For vehicle {vehicleModel.Id} position update while docked, will not execute");
                        return;
                    }

                    switch (vehicle)
                    {
                        case SeaMoth seamoth:
                            {
                                mvc = seamoth.gameObject.EnsureComponent<MultiplayerSeaMoth>();
                                break;
                            }
                        case Exosuit exosuit:
                            {
                                mvc = exosuit.gameObject.EnsureComponent<MultiplayerExosuit>();

                                if (vehicleModel is ExosuitMovementData exoSuitMovement)
                                {
                                    mvc.SetArmPositions(exoSuitMovement.LeftAimTarget.ToUnity(), exoSuitMovement.RightAimTarget.ToUnity());
                                }
                                else
                                {
                                    Log.Error($"{nameof(Vehicles)}: Got exosuit vehicle but no ExosuitMovementData");
                                }
                                break;
                            }
                    }

                }
                else if (rocket)
                {
                    rocket.transform.position = vehicleModel.Position.ToUnity();
                    rocket.transform.rotation = vehicleModel.Rotation.ToUnity();
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

            // TODO: REMOVE THIS; Set the vehicle/subroot/chair in appropriate patches and packet send - Fixing it with movement packets will have unfortunate timing with animations (like vehicle docking). Same applies to RemotePlayer.UpdatePosition.
            if (player.HasValue)
            {
                RemotePlayer playerInstance = player.Value;
                playerInstance.SetVehicle(vehicle);
                playerInstance.SetSubRoot(subRoot);
                playerInstance.SetPilotingChair(subRoot.AliveOrNull()?.GetComponentInChildren<PilotingChair>());
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
        }

        public void DestroyVehicle(NitroxId id)
        {
            Optional<GameObject> Object = NitroxEntity.GetObjectFrom(id);
            if (Object.HasValue)
            {
                GameObject go = Object.Value;
                Vehicle vehicle = go.RequireComponent<Vehicle>();

                if (vehicle.GetPilotingMode()) //Check Local Object Have Player inside
                {
                    vehicle.OnPilotModeEnd();

                    if (!Player.main.ToNormalMode(true))
                    {
                        Player.main.ToNormalMode(false);
                        Player.main.transform.parent = null;
                    }
                }
                foreach (RemotePlayerIdentifier identifier in vehicle.GetComponentsInChildren<RemotePlayerIdentifier>(true))
                {
                    identifier.RemotePlayer.ResetStates();
                }

                //Destroy vehicle
                if (vehicle.gameObject)
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

        public void BroadcastDestroyedVehicle(Vehicle vehicle)
        {
            NitroxId id = NitroxEntity.GetId(vehicle.gameObject);

            VehicleDestroyed vehicleDestroyed = new(id);
            packetSender.SendIfGameCode(vehicleDestroyed);
        }

        public void BroadcastVehicleDocking(VehicleDockingBay dockingBay, Vehicle vehicle)
        {
            NitroxId dockId = NitroxEntity.GetId(dockingBay.gameObject);


            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            packetSender.SendIfGameCode<VehicleDocking>(new(vehicleId, dockId, playerId));
        }

        public void BroadcastVehicleUndocking(VehicleDockingBay dockingBay, Vehicle vehicle, bool isUndockStarting)
        {
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            NitroxId dockId = NitroxEntity.GetId(dockingBay.gameObject);
            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
            VehicleUndocking packet = new(vehicleId, dockId, playerId, isUndockStarting);
            packetSender.SendIfGameCode(packet);
        }

        public IEnumerator UpdateVehiclePositionAfterSpawn(NitroxId id, TechType techType, GameObject gameObject, float cooldown)
        {
            yield return new WaitForSeconds(cooldown);

            VehicleMovementData vehicleMovementData = new BasicVehicleMovementData(techType.ToDto(), id, gameObject.transform.position.ToDto(), gameObject.transform.rotation.ToDto());
            ushort playerId = ushort.MaxValue;

            packetSender.Send(new VehicleMovement(playerId, vehicleMovementData));
        }

        public void BroadcastOnPilotModeChanged(Vehicle vehicle, bool isPiloting)
        {
            ushort playerId = multiplayerSession.Reservation.PlayerId;
            packetSender.SendIfGameCode<VehicleOnPilotModeChanged>(new(NitroxEntity.GetId(vehicle.gameObject), playerId, isPiloting));
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

        public bool RemoveVehicle(NitroxId id)
        {
            return vehiclesById.Remove(id);
        }

        public T GetVehicle<T>(NitroxId vehicleId) where T : VehicleModel
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
