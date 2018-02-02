using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerLogic // Named as such because UWE's 'Player' pollutes the global namespace :(
    {
        private readonly IPacketSender packetSender;

        public PlayerLogic(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(packetSender.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }

        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleModel> opVehicle, Optional<string> opSubGuid)
        {
            Movement movement;

            if (opVehicle.IsPresent())
            {
                VehicleModel vehicle = opVehicle.Get();
                movement = new VehicleMovement(packetSender.PlayerId, vehicle.Position, vehicle.Velocity, vehicle.Rotation, vehicle.AngularVelocity, vehicle.TechType, vehicle.Guid, vehicle.SteeringWheelYaw, vehicle.SteeringWheelPitch, vehicle.AppliedThrottle);
            }
            else
            {
                movement = new Movement(packetSender.PlayerId, location, velocity, bodyRotation, aimingRotation, opSubGuid);
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(packetSender.PlayerId, (int)type, (int)state);
            packetSender.Send(animEvent);
        }
    }
}
