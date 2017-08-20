using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : ClientPacketProcessor<VehicleMovement>
    {
        private PlayerManager remotePlayerManager;

        public VehicleMovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }
        
        public override void Process(VehicleMovement vehicleMovement)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicleMovement.Guid);

            Vector3 remotePosition = ApiHelper.Vector3(vehicleMovement.PlayerPosition);
            Vector3 remoteVelocity = ApiHelper.Vector3(vehicleMovement.Velocity);
            Quaternion remoteRotation = ApiHelper.Quaternion(vehicleMovement.BodyRotation);
            Vector3 remoteAngularVelocity = ApiHelper.Vector3(vehicleMovement.AngularVelocity);

            if (opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();

                Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

                if (rigidbody != null)
                {
                    //todo: maybe toggle kinematic if jumping large distances?
                    rigidbody.velocity = GetVehicleVelocity(remotePosition, remoteVelocity, gameObject);
                    rigidbody.angularVelocity = GetVehicleAngularVelocity(remoteRotation, remoteAngularVelocity, gameObject);
                }
                else
                {
                    Console.WriteLine("Vehicle did not have a rigidbody!");
                }
            }
            else
            {
                CreateVehicleAt(vehicleMovement.TechType, vehicleMovement.Guid, remotePosition, remoteRotation);
            }

            RemotePlayer remotePlayer = remotePlayerManager[vehicleMovement.PlayerId];
            remotePlayer.animationController.UpdatePlayerAnimations = false;
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
                GameObject techPrefab = CraftData.GetPrefabForTechType(techType, false);

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

        /*
         * For the cyclops, it is too intense for the game to lerp the entire structure every movement
         * packet update.  Instead, we try to match the velocity.  Due to floating points not being
         * precise, this will skew quickly.  To counter this, we apply micro adjustments each packet
         * to get the simulation back in sync.  The adjustments will increase in size the larger the  
         * out of sync issue is.
         */
        private Vector3 GetVehicleVelocity(Vector3 remotePosition, Vector3 remoteVelocity, GameObject gameObject)
        {
            Vector3 difference = (remotePosition - gameObject.transform.position);
            Vector3 velocityToMakeUpDifference = difference / PlayerMovement.BROADCAST_INTERVAL;

            float distance = Vector3.Distance(remotePosition, gameObject.transform.position);
            float maxAdjustment = 0.15f;

            if (distance > 10)
            {
                maxAdjustment = 1f;
            }
            else if (distance > 5)
            {
                maxAdjustment = 0.5f;
            }
            else if (distance > 1)
            {
                maxAdjustment = 0.25f;
            }
            else if (distance < 0.1 && remoteVelocity == Vector3.zero) //overcorrections can cause jitter when standing still. 
            {
                return Vector3.zero;
            }

            Vector3 limitedVelocityChange = MathUtil.ClampMagnitude(velocityToMakeUpDifference - remoteVelocity, maxAdjustment, maxAdjustment * -1);

            return remoteVelocity + limitedVelocityChange;
        }

        private Vector3 GetVehicleAngularVelocity(Quaternion remoteRotation, Vector3 remoteAngularVelocity, GameObject gameObject)
        {
            Quaternion delta = remoteRotation * Quaternion.Inverse(gameObject.transform.rotation);

            float angle; Vector3 axis;
            delta.ToAngleAxis(out angle, out axis);

            // We get an infinite axis in the event that our rotation is already aligned.
            if (float.IsInfinity(axis.x))
            {
                return Vector3.zero;
            }

            if (angle > 180f)
            {
                angle -= 360f;
            }

            // Here I drop down to 0.9f times the desired movement,
            // since we'd rather undershoot and ease into the correct angle
            // than overshoot and oscillate around it in the event of errors.
            return (0.9f * Mathf.Deg2Rad * angle / PlayerMovement.BROADCAST_INTERVAL) * axis.normalized;            
        }
    }
}
