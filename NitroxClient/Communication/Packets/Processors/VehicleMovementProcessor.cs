using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : GenericPacketProcessor<VehicleMovement>
    {
        private MultiplayerObjectManager multiplayerObjectManager;
        private PlayerGameObjectManager playerGameObjectManager;

        public VehicleMovementProcessor(MultiplayerObjectManager multiplayerObjectManager, PlayerGameObjectManager playerGameObjectManager)
        {
            this.multiplayerObjectManager = multiplayerObjectManager;
            this.playerGameObjectManager = playerGameObjectManager;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            playerGameObjectManager.HidePlayerGameObject(vehicleMovement.PlayerId);

            Optional<GameObject> opGameObject = multiplayerObjectManager.GetManagedObject(vehicleMovement.Guid);

            if(opGameObject.IsEmpty())
            {
                opGameObject = createVehicle(vehicleMovement.TechType, vehicleMovement.Guid);

                if(opGameObject.IsEmpty())
                {
                    return;
                }

                multiplayerObjectManager.SetupManagedObject(vehicleMovement.Guid, opGameObject.Get());
            }

            GameObject gameObject = opGameObject.Get();
            Vector3 position = ApiHelper.Vector3(vehicleMovement.PlayerPosition);
            Quaternion rotation = ApiHelper.Quaternion(vehicleMovement.Rotation);
            MovementHelper.MoveGameObject(gameObject, position, rotation);
        }

        private Optional<GameObject> createVehicle(String techTypeString, String guid)
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

                ManagedMultiplayerObject managedObject = gameObject.AddComponent<ManagedMultiplayerObject>();
                managedObject.ChangeGuid(guid);

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
