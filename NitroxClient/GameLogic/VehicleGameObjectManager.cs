using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class VehicleGameObjectManager
    {
        private Dictionary<String, Optional<GameObject>> vehiclesByGuid = new Dictionary<String, Optional<GameObject>>();

        public void UpdateVehiclePosition(String guid, String techTypeString, Vector3 position, Quaternion rotation)
        {
            Optional<GameObject> opVehicle = GetVehicleGameObject(guid, techTypeString);

            if (opVehicle.IsPresent())
            {
                opVehicle.Get().transform.position = position;
                opVehicle.Get().transform.rotation = rotation;
            }
        }

        public Optional<GameObject> GetVehicleGameObject(String guid, String techTypeString)
        {
            if (!vehiclesByGuid.ContainsKey(guid))
            {
                TechType techType;
                Optional<GameObject> vehicle = Optional<GameObject>.Empty();

                if (UWE.Utils.TryParseEnum<TechType>(techTypeString, out techType))
                {
                    vehicle = createVehicle(techType, guid);
                }
                else
                {
                    Console.WriteLine("Unknown tech type: " + techTypeString);
                }

                vehiclesByGuid[guid] = vehicle;
            }


            return vehiclesByGuid[guid];
        }
        
        private Optional<GameObject> createVehicle(TechType techType, String guid)
        {
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
