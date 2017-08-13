using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : ClientPacketProcessor<VehicleMovement>
    {
        private const int VEHICLE_POS_CORRECTION_AFTER_N_PACKETS = 60;

        private PlayerManager remotePlayerManager;
        private Dictionary<String, int> correctionCounter = new Dictionary<String, int>(); //TODO: TTL

        public VehicleMovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicleMovement.Guid);

            Vector3 position = ApiHelper.Vector3(vehicleMovement.PlayerPosition);
            Quaternion rotation = ApiHelper.Quaternion(vehicleMovement.BodyRotation);

            if (opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();

                /*
                    * For the cyclops, it is too intense for the game to lerp 
                    * the entire structure every movement packet update.  
                    * Instead, we try to match the velocity.  Due to floating
                    * points not being precise, this will skew quickly.  To 
                    * counter this, apply a correction every n-number of packets.
                    */

                if(!correctionCounter.ContainsKey(vehicleMovement.Guid))
                {
                    correctionCounter.Add(vehicleMovement.Guid, 0);
                }

                int currentCorrectionCounter = correctionCounter[vehicleMovement.Guid];

                if(currentCorrectionCounter == VEHICLE_POS_CORRECTION_AFTER_N_PACKETS)
                {
                    gameObject.transform.position = position;
                    gameObject.transform.rotation = rotation;
                }

                Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                rigidbody.velocity = ApiHelper.Vector3(vehicleMovement.Velocity);
                rigidbody.angularVelocity = ApiHelper.Vector3(vehicleMovement.AngularVelocity);
            }
            else
            {
                CreateVehicleAt(vehicleMovement.TechType, vehicleMovement.Guid, position, rotation);
            }
        }

        private void CreateVehicleAt(String techTypeString, String guid, Vector3 position, Quaternion rotation)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(techTypeString);

            if (opTechType.IsEmpty())
            {
                Console.WriteLine("Unknown tech type: " + techTypeString);
                return;
            }

            TechType techType = opTechType.Get();
            
            if (techType == TechType.Cyclops)
            {
                LightmappedPrefabs.main.RequestScenePrefab("cyclops", (go) => OnVehiclePrefabLoaded(go, guid, position, rotation));
            }
            else
            {
                GameObject techPrefab = TechTree.main.GetGamePrefab(techType);

                if (techPrefab != null)
                {
                    OnVehiclePrefabLoaded(techPrefab, guid, position, rotation);
                }
                else
                {
                    Console.WriteLine("No prefab for tech type: " + techType);
                }
            }
        }

        private void OnVehiclePrefabLoaded(GameObject prefab, string guid, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            // Partially copied from SubConsoleCommand.OnSubPrefabLoaded
            GameObject gameObject = Utils.SpawnPrefabAt(prefab, null, spawnPosition);
            gameObject.transform.rotation = spawnRotation;
            gameObject.SetActive(true);
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));

            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody.isKinematic = false;

            GuidHelper.SetNewGuid(gameObject, guid);
        }
    }
}
