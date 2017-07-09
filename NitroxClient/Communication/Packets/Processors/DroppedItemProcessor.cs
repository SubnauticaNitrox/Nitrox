using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DroppedItemProcessor : GenericPacketProcessor<DroppedItem>
    {
        private MultiplayerObjectManager multiplayerObjectManager;

        public DroppedItemProcessor(MultiplayerObjectManager multiplayerObjectManager)
        {
            this.multiplayerObjectManager = multiplayerObjectManager;
        }

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

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.isKinematic = false;
                rigidBody.AddForce(ApiHelper.Vector3(drop.PushVelocity), ForceMode.VelocityChange);

                multiplayerObjectManager.SetupManagedObject(drop.Guid, gameObject);
            }

            //TODO: TechType specific logic when dropped, ex: constructor.deploy();
        }
    }
}
