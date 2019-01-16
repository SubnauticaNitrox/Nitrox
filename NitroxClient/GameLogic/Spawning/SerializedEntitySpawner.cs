using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class SerializedEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            GameObject gameObject = SerializationHelper.GetGameObject(entity.SerializedGameObject);
            gameObject.transform.position = entity.Position;
            gameObject.transform.rotation = entity.Rotation;
            gameObject.transform.localScale = entity.Scale;
            
            if (entity.WaterParkGuid != null)
            {
                AssignToWaterPark(gameObject, entity.WaterParkGuid);
            }

            EnableRigidBody(gameObject);
            ExecuteDropItemAction(entity.TechType, gameObject);

            return Optional<GameObject>.Of(gameObject);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
        
        private void EnableRigidBody(GameObject gameObject)
        {
            Rigidbody rigidBody = gameObject.RequireComponent<Rigidbody>();
            rigidBody.isKinematic = false;
        }

        // The next two functions could potentially reside outside of this specific serializer.  
        // They only happen to be in here because dropped items use this code path.

        private void AssignToWaterPark(GameObject gameObject, string waterParkGuid)
        {
            Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
            GameObject waterParkGo = GuidHelper.RequireObjectFrom(waterParkGuid);
            WaterPark waterPark = waterParkGo.RequireComponent<WaterPark>();

            waterPark.AddItem(pickupable);
        }

        private void ExecuteDropItemAction(TechType techType, GameObject gameObject)
        {
            Log.Debug("Performing drop action for tech type: " + techType);

            ItemDropAction itemDropAction = ItemDropAction.FromTechType(techType);
            itemDropAction.ProcessDroppedItem(gameObject);
        }

    }
}
