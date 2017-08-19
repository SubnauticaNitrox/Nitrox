using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;
using System.Collections.Generic;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.Communication
{
    public class PacketSender
    {
        public bool Active { get; set; }
        public String PlayerId { get; set; }

        private TcpClient client;
        private HashSet<Type> suppressedPacketsTypes;

        public PacketSender(TcpClient client)
        {
            this.client = client;
            this.Active = false;
            this.suppressedPacketsTypes = new HashSet<Type>();
        }

        public void Authenticate()
        {
            Authenticate auth = new Authenticate(PlayerId);
            Send(auth);
        }

        public void UpdatePlayerLocation(Vector3 location, Quaternion bodyRotation, Quaternion cameraRotation, Optional<VehicleModel> opVehicle, Optional<String> opSubGuid)
        {
            Movement movement;

            if (opVehicle.IsPresent())
            {
                VehicleModel vehicle = opVehicle.Get();
                movement = new VehicleMovement(PlayerId, vehicle.Position, vehicle.Rotation, vehicle.Velocity, vehicle.AngularVelocity, vehicle.TechType, vehicle.Guid);
            }
            else
            {
                movement = new Movement(PlayerId, ApiHelper.Vector3(location), ApiHelper.Quaternion(bodyRotation), ApiHelper.Quaternion(cameraRotation), opSubGuid);
            }

            Send(movement);
        }
        
        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(PlayerId, (int)type, (int)state);
            Send(animEvent);
        }

        public void Send(Packet packet)
        {
            if (Active && !suppressedPacketsTypes.Contains(packet.GetType()))
            {
                try
                {
                    client.Send(packet);
                }
                catch (Exception ex)
                {
                    ErrorMessage.AddError($"Error sending {packet}: {ex.Message}");
                    Console.WriteLine("Error sending packet {0}\n{1}", packet, ex);
                }
            }
        }

        public void AddSuppressedPacketType(Type type)
        {
            suppressedPacketsTypes.Add(type);
        }

        public void RemoveSuppressedPacketType(Type type)
        {
            suppressedPacketsTypes.Remove(type);
        }
    }
}
