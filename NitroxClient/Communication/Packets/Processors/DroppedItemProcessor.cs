using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DroppedItemProcessor : ClientPacketProcessor<DroppedItem>
    {
        private readonly INitroxLogger log;

        public DroppedItemProcessor(INitroxLogger logger)
        {
            log = logger;
        }

        public override void Process(DroppedItem drop)
        {
            GameObject gameObject = SerializationHelper.GetGameObject(drop.Bytes);
            gameObject.transform.position = drop.ItemPosition;

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();

            if (drop.WaterParkGuid.IsPresent())
            {
                AssignToWaterPark(drop.WaterParkGuid.Get(), pickupable);
            }

            EnableRigidBody(gameObject);
            ExecuteDropItemAction(drop.TechType, gameObject);
            RemoveExistingSyncScripts(gameObject);
        }

        private void EnableRigidBody(GameObject gameObject)
        {
            Rigidbody rigidBody = gameObject.RequireComponent<Rigidbody>();
            rigidBody.isKinematic = false;
        }

        private void AssignToWaterPark(string waterParkGuid, Pickupable pickupable)
        {
            GameObject waterParkGo = GuidHelper.RequireObjectFrom(waterParkGuid);
            WaterPark waterPark = waterParkGo.RequireComponent<WaterPark>();

            waterPark.AddItem(pickupable);
        }

        private void ExecuteDropItemAction(TechType techType, GameObject gameObject)
        {
            log.Debug($"Performing drop action for tech type: {techType}");

            ItemDropAction itemDropAction = ItemDropAction.FromTechType(techType);
            itemDropAction.ProcessDroppedItem(gameObject);
        }

        private void RemoveExistingSyncScripts(GameObject gameObject)
        {
            SyncedMultiplayerObject smo = gameObject.GetComponent<SyncedMultiplayerObject>();

            if (smo != null)
            {
                Object.Destroy(smo);
            }
        }
    }
}
