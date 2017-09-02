using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
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

            if (vehicle != null)
            {
                guid = GuidHelper.GetGuid(vehicle.gameObject);
                position = vehicle.gameObject.transform.position;
                rotation = vehicle.gameObject.transform.rotation;
                techType = CraftData.GetTechType(vehicle.gameObject);

                Rigidbody rigidbody = vehicle.gameObject.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
            }
            else if (sub != null && Player.main.isPiloting)
            {
                guid = GuidHelper.GetGuid(sub.gameObject);
                position = sub.gameObject.transform.position;
                rotation = sub.gameObject.transform.rotation;
                Rigidbody rigidbody = sub.gameObject.GetComponent<Rigidbody>();
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                techType = TechType.Cyclops;
            }
            else
            {
                return Optional<VehicleModel>.Empty();
            }

            VehicleModel model = new VehicleModel(Enum.GetName(typeof(TechType), techType),
                                                  guid,
                                                  ApiHelper.Vector3(position),
                                                  ApiHelper.Quaternion(rotation),
                                                  ApiHelper.Vector3(velocity));

            return Optional<VehicleModel>.Of(model);
        }
    }
}
