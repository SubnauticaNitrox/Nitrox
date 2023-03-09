using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
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

namespace NitroxClient.GameLogic
{
    public class Vehicles
    {
        private readonly IPacketSender packetSender;
        private readonly PlayerManager playerManager;
        private readonly IMultiplayerSession multiplayerSession;
        private readonly SimulationOwnership simulationOwnership;

        public Vehicles(IPacketSender packetSender, PlayerManager playerManager, IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnership)
        {
            this.packetSender = packetSender;
            this.playerManager = playerManager;
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnership = simulationOwnership;
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

            if (player.HasValue)
            {
                RemotePlayer playerInstance = player.Value;
                playerInstance.SetVehicle(vehicle);
                playerInstance.SetSubRoot(subRoot);
                playerInstance.SetPilotingChair(subRoot.AliveOrNull()?.GetComponentInChildren<PilotingChair>());
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
        }

        public void BroadcastDestroyedVehicle(NitroxId id)
        {
            using (PacketSuppressor<VehicleOnPilotModeChanged>.Suppress())
            {
                EntityDestroyed vehicleDestroyed = new(id);
                packetSender.Send(vehicleDestroyed);
            }
        }

        public void BroadcastVehicleDocking(VehicleDockingBay dockingBay, Vehicle vehicle)
        {
            NitroxId dockId = NitroxEntity.GetId(dockingBay.gameObject);


            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            VehicleDocking packet = new VehicleDocking(vehicleId, dockId, playerId);
            packetSender.Send(packet);

            PacketSuppressor<PlayerMovement> playerMovementSuppressor = PacketSuppressor<PlayerMovement>.Suppress();
            PacketSuppressor<VehicleMovement> vehicleMovementSuppressor = PacketSuppressor<VehicleMovement>.Suppress();
            vehicle.StartCoroutine(AllowMovementPacketsAfterDockingAnimation(playerMovementSuppressor, vehicleMovementSuppressor));
        }

        public void BroadcastVehicleUndocking(VehicleDockingBay dockingBay, Vehicle vehicle, bool undockingStart)
        {
            NitroxId dockId = NitroxEntity.GetId(dockingBay.gameObject);

            NitroxId vehicleId = NitroxEntity.GetId(vehicle.gameObject);
            ushort playerId = multiplayerSession.Reservation.PlayerId;

            PacketSuppressor<PlayerMovement> movementSuppressor = PacketSuppressor<PlayerMovement>.Suppress();
            PacketSuppressor<VehicleMovement> vehicleMovementSuppressor = PacketSuppressor<VehicleMovement>.Suppress();
            if (!undockingStart)
            {
                movementSuppressor.Dispose();
                vehicleMovementSuppressor.Dispose();
            }

            VehicleUndocking packet = new VehicleUndocking(vehicleId, dockId, playerId, undockingStart);
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
        public IEnumerator AllowMovementPacketsAfterDockingAnimation(PacketSuppressor<PlayerMovement> playerMovementSuppressor, PacketSuppressor<VehicleMovement> vehicleMovementSuppressor)
        {
            yield return Yielders.WaitFor3Seconds;
            playerMovementSuppressor.Dispose();
            vehicleMovementSuppressor.Dispose();
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

            VehicleOnPilotModeChanged packet = new(NitroxEntity.GetId(vehicle.gameObject), playerId, isPiloting);
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

        /// <summary>
        /// Subnautica pre-emptively loads a prefab of each vehicle (such as a cyclops) during the initial game load.  This allows the game to instantaniously
        /// use this prefab for the first constructor event.  Subsequent constructor events will use this prefab as a template.  However, this is problematic
        /// because the template + children are now tagged with NitroxEntity because players are interacting with it. We need to remove any NitroxEntity from
        /// the new gameObject that used the template.
        /// </summary>
        public static void RemoveNitroxEntityTagging(GameObject constructedObject)
        {
            NitroxEntity[] nitroxEntities = constructedObject.GetComponentsInChildren<NitroxEntity>(true);

            foreach (NitroxEntity nitroxEntity in nitroxEntities)
            {
                UnityEngine.Object.DestroyImmediate(nitroxEntity);
            }
        }
    }
}
