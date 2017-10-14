using NitroxClient.Communication;
using NitroxModel.DataStructures.Util;
using NitroxClient.GameLogic.Helper;
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

            PlaceBasePiece placedBasePiece = new PlaceBasePiece(packetSender.PlayerId, guid, itemPosition, quaternion, camera.position, camera.rotation, techType, Optional<String>.OfNullable(parentBaseGuid));
            packetSender.Send(placedBasePiece);
        }

        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            String guid = GuidHelper.GetGuid(gameObject);

            Optional<String> subGuid = Optional<String>.Empty();
            var sub = Player.main.currentSub;
            if (sub != null)
            {
                subGuid = Optional<String>.Of(GuidHelper.GetGuid(sub.gameObject));
            }

            Transform camera = Camera.main.transform;

            PlaceFurniture placedFurniture = new PlaceFurniture(packetSender.PlayerId, guid, subGuid, itemPosition, quaternion, camera.position, camera.rotation, techType);
            packetSender.Send(placedFurniture);
        }

        public void ChangeConstructionAmount(GameObject gameObject, float amount)
        {
            timeSinceLastConstructionChangeEvent += Time.deltaTime;

            if (timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
            {
                return;
            }

            timeSinceLastConstructionChangeEvent = 0.0f;

            Vector3 itemPosition = gameObject.transform.position;
            String guid = GuidHelper.GetGuid(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(packetSender.PlayerId, itemPosition, guid, amount);
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

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(packetSender.PlayerId, itemPosition, guid, newlyConstructedBaseGuid);
            packetSender.Send(constructionCompleted);
        }

        public void DeconstructionBegin(GameObject gameObject)
        {
            Vector3 itemPosition = gameObject.transform.position;
            String guid = GuidHelper.GetGuid(gameObject);

            DeconstructionBegin deconstructionBegin = new DeconstructionBegin(packetSender.PlayerId, itemPosition, guid);
            packetSender.Send(deconstructionBegin);
        }

        public void DeconstructionComplete(GameObject gameObject)
        {
            Vector3 itemPosition = gameObject.transform.position;
            String guid = GuidHelper.GetGuid(gameObject);

            DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(packetSender.PlayerId, itemPosition, guid);
            packetSender.Send(deconstructionCompleted);
        }
    }
}
