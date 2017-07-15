using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : ClientPacketProcessor<VehicleMovement>
    {
        private const float VEHICLE_TRANSFORM_SMOOTH_PERIOD = 0.05f;
        
        private PlayerManager remotePlayerManager;

        public VehicleMovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicleMovement.Guid);

            if(opGameObject.IsEmpty())
            {
                opGameObject = CreateVehicle(vehicleMovement.TechType, vehicleMovement.Guid);

                if(opGameObject.IsEmpty())
                {
                    return;
                }
                
                GuidHelper.SetNewGuid(opGameObject.Get(), vehicleMovement.Guid);
            }

            GameObject gameObject = opGameObject.Get();
            Vector3 position = ApiHelper.Vector3(vehicleMovement.PlayerPosition);
            Quaternion rotation = ApiHelper.Quaternion(vehicleMovement.Rotation);
            MovementHelper.MoveGameObject(gameObject, position, rotation, VEHICLE_TRANSFORM_SMOOTH_PERIOD);
        }

        private Optional<GameObject> CreateVehicle(String techTypeString, String guid)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(techTypeString);
            
            if (opTechType.IsEmpty())
            {
                Console.WriteLine("Unknown tech type: " + techTypeString);
                return Optional<GameObject>.Empty();
            }

            TechType techType = opTechType.Get();

            GameObject techPrefab = TechTree.main.GetGamePrefab(techType);

            if (techPrefab != null)
            {
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(techPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.up, Vector3.up));
                gameObject.SetActive(true);
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.isKinematic = false;
                
                GuidHelper.SetNewGuid(gameObject, guid);

                return Optional<GameObject>.Of(gameObject);
            }
            else
            {
                Console.WriteLine("No prefab for tech type: " + techType);
            }

            return Optional<GameObject>.Empty();
        }
    }
}
