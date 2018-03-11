using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.Rendering;

namespace NitroxClient.GameLogic
{
    public class LocalPlayer : ILocalNitroxPlayer
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;

        public GameObject Body { get; }
        public GameObject PlayerModel { get; }
        public GameObject BodyPrototype { get; }
        public string PlayerName => multiplayerSession.AuthenticationContext.Username;
        public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;

            Body = Player.main.transform.RequireTransform("body").gameObject;
            PlayerModel = Body.transform.RequireTransform("player_view").gameObject;

            BodyPrototype = CreateBodyPrototype();
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(multiplayerSession.Reservation.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }

        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleModel> opVehicle, Optional<string> opSubGuid)
        {
            Movement movement;

            if (opVehicle.IsPresent())
            {
                VehicleModel vehicle = opVehicle.Get();
                movement = new VehicleMovement(multiplayerSession.Reservation.PlayerId, vehicle.Position, vehicle.Velocity, vehicle.Rotation, vehicle.AngularVelocity, vehicle.TechType, vehicle.Guid, vehicle.SteeringWheelYaw, vehicle.SteeringWheelPitch,
                    vehicle.AppliedThrottle);
            }
            else
            {
                movement = new Movement(multiplayerSession.Reservation.PlayerId, location, velocity, bodyRotation, aimingRotation, opSubGuid);
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(multiplayerSession.Reservation.PlayerId, (int)type, (int)state);
            packetSender.Send(animEvent);
        }

        public void BroadcastDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(multiplayerSession.Reservation.PlayerId, deathPosition);
            packetSender.Send(playerDeath);
        }

        private GameObject CreateBodyPrototype()
        {
            GameObject prototype = Body;

            // Cheap fix for showing head, much easier since male_geo contains many different heads
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
            GameObject clone = Object.Instantiate(prototype);
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            return clone;
        }
    }
}
