using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f;

        private float time = 0.0f;

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Vector3 playerVelocity = Player.main.playerController.velocity;

                //if (Player.main.groundMotor.IsGrounded())
                ////if (Player.main.playerController.activeController == Player.main.groundMotor && Player.main.groundMotor.IsGrounded())
                //{
                //    playerVelocity.y = 0f;
                //}

                // IDEA: possibly only CameraRotation is of interest, because bodyrotation is extracted from that.
                // WARN: Using camera rotation may be dangerous, when the drone is used for instance (but then movement packets shouldn't be sent either so it's not even relevant...)

                Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
                Quaternion aimingRotation = Player.main.camRoot.GetAimingTransform().rotation;

                Optional<VehicleModel> vehicle = GetVehicleModel();
                String subGuid = null;

                SubRoot currentSub = Player.main.GetCurrentSub();

                if (currentSub != null)
                {
                    subGuid = GuidHelper.GetGuid(currentSub.gameObject);
                }

                Multiplayer.PacketSender.UpdatePlayerLocation(currentPosition, playerVelocity, bodyRotation, aimingRotation, vehicle, Optional<String>.OfNullable(subGuid));
            }
        }

        private Optional<VehicleModel> GetVehicleModel()
        {
            Vehicle vehicle = Player.main.GetVehicle();
            SubRoot sub = Player.main.GetCurrentSub();

            String guid;
            Vector3 position;
            Quaternion rotation;
            Vector3 velocity;
            Vector3 angularVelocity;
            TechType techType;
            float steeringWheelYaw = 0f, steeringWheelPitch = 0f;
            bool appliedThrottle = false;

            if (vehicle != null)
            {
                guid = GuidHelper.GetGuid(vehicle.gameObject);
                position = vehicle.gameObject.transform.position;
                rotation = vehicle.gameObject.transform.rotation;
                techType = CraftData.GetTechType(vehicle.gameObject);

                Rigidbody rigidbody = vehicle.gameObject.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;

                // Required because vehicle is either a SeaMoth or an Exosuit, both types which can't see the fields either.
                steeringWheelYaw = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelYaw");
                steeringWheelPitch = (float)vehicle.ReflectionGet<Vehicle, Vehicle>("steeringWheelPitch");

                // TODO: Check if Vehicles have such a thing as well.
            }
            else if (sub != null && Player.main.isPiloting)
            {
                guid = GuidHelper.GetGuid(sub.gameObject);
                position = sub.gameObject.transform.position;
                rotation = sub.gameObject.transform.rotation;
                Rigidbody rigidbody = sub.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                techType = TechType.Cyclops;

                var scr = sub.GetComponent<SubControl>();
                steeringWheelYaw = (float)scr.ReflectionGet("steeringWheelYaw");
                steeringWheelPitch = (float)scr.ReflectionGet("steeringWheelPitch");
                appliedThrottle = scr.appliedThrottle && (bool)scr.ReflectionGet("canAccel");
            }
            else
            {
                return Optional<VehicleModel>.Empty();
            }

            VehicleModel model = new VehicleModel(techType,
                                                  guid,
                                                  position,
                                                  rotation,
                                                  velocity,
                                                  steeringWheelYaw,
                                                  steeringWheelPitch,
                                                  appliedThrottle);

            return Optional<VehicleModel>.Of(model);
        }
    }
}
