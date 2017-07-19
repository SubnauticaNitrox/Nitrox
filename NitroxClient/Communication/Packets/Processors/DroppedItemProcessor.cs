using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DroppedItemProcessor : GenericPacketProcessor<DroppedItem>
    {
        public override void Process(DroppedItem drop)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(drop.TechType);

            if(opTechType.IsEmpty())
            {
                Console.WriteLine("Attempted to drop unknown tech type: " + drop.TechType);
                return;
            }

            TechType techType = opTechType.Get();
            
            GameObject techPrefab = TechTree.main.GetGamePrefab(techType);

            if (techPrefab != null)
            {
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(techPrefab, ApiHelper.Vector3(drop.ItemPosition), Quaternion.FromToRotation(Vector3.up, Vector3.up));
                gameObject.SetActive(true);
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                
                GuidHelper.SetNewGuid(gameObject, drop.Guid);

                ItemDropAction itemDropAction = ItemDropAction.FromTechType(techType);
                itemDropAction.ProcessDroppedItem(gameObject);
            }
        }
    }
}
