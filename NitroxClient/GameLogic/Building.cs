using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class Building
    {
        private const float CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS = 0.10f;

        private PacketSender packetSender;
        private float timeSinceLastConstructionChangeEvent = 0.0f;

        public Building(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        
        public void PlaceBasePiece(ConstructableBase constructableBase, Base targetBase, TechType techType, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            String guid = GuidHelper.GetGuid(constructableBase.gameObject);
            String parentBaseGuid = (targetBase == null) ? null : GuidHelper.GetGuid(targetBase.gameObject);
            Vector3 itemPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;

            PlaceBasePiece placedBasePiece = new PlaceBasePiece(packetSender.PlayerId, guid, ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), ApiHelper.Transform(camera), ApiHelper.TechType(techType), Optional<String>.OfNullable(parentBaseGuid));
            packetSender.Send(placedBasePiece);
        }
        
        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            String guid = GuidHelper.GetGuid(gameObject);
            String subGuid = GuidHelper.GetGuid(Player.main.GetCurrentSub().gameObject);
            Transform camera = Camera.main.transform;

            PlaceFurniture placedFurniture = new PlaceFurniture(packetSender.PlayerId, guid, subGuid, ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), ApiHelper.Transform(camera), ApiHelper.TechType(techType));
            packetSender.Send(placedFurniture);
        }
        
        public void ChangeConstructionAmount(GameObject gameObject, float amount)
        {
            timeSinceLastConstructionChangeEvent += Time.deltaTime;

            if (IsConstructionPacketEcho(gameObject) || timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
            {
                return;
            }

            timeSinceLastConstructionChangeEvent = 0.0f;

            Vector3 itemPosition = gameObject.transform.position;
            String guid = GuidHelper.GetGuid(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(packetSender.PlayerId, ApiHelper.Vector3(itemPosition), guid, amount);
                packetSender.Send(amountChanged);
            }
        }
       
        public void ConstructionComplete(GameObject gameObject)
        {
            Optional<String> newlyConstructedBaseGuid = Optional<String>.Empty();
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opConstructedBase.IsPresent())
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Get();
                newlyConstructedBaseGuid = Optional<String>.Of(GuidHelper.GetGuid(constructedBase));
            }

            Vector3 itemPosition = gameObject.transform.position;
            String guid = GuidHelper.GetGuid(gameObject);

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(packetSender.PlayerId, ApiHelper.Vector3(itemPosition), guid, newlyConstructedBaseGuid);
            packetSender.Send(constructionCompleted);
        }

        private bool IsConstructionPacketEcho(GameObject gameObject)
        {
            PlayerTool playerTool = Inventory.main.GetHeldTool();

            if (playerTool is BuilderTool)
            {
                Targeting.AddToIgnoreList(Player.main.gameObject);
                GameObject target;
                float num;
                Targeting.GetTarget(30f, out target, out num, null);

                if (target == null)
                {
                    return true;
                }

                Constructable constructable = target.GetComponentInParent<Constructable>();

                return (constructable.gameObject != gameObject);
            }

            return true;
        }

    }
}
