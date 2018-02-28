﻿using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class Building
    {
        private const float CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS = 0.10f;

        private readonly IPacketSender packetSender;
        private float timeSinceLastConstructionChangeEvent;

        public Building(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void PlaceBasePiece(ConstructableBase constructableBase, Base targetBase, TechType techType, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            string guid = GuidHelper.GetGuid(constructableBase.gameObject);
            string parentBaseGuid = (targetBase == null) ? null : GuidHelper.GetGuid(targetBase.gameObject);
            Vector3 itemPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;

            PlaceBasePiece placedBasePiece = new PlaceBasePiece(guid, itemPosition, quaternion, camera.position, camera.rotation, techType, Optional<string>.OfNullable(parentBaseGuid));
            packetSender.Send(placedBasePiece);
        }

        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            string guid = GuidHelper.GetGuid(gameObject);

            Optional<string> subGuid = Optional<string>.Empty();
            SubRoot sub = Player.main.currentSub;
            if (sub != null)
            {
                subGuid = Optional<string>.Of(GuidHelper.GetGuid(sub.gameObject));
            }

            Transform camera = Camera.main.transform;

            PlaceFurniture placedFurniture = new PlaceFurniture(guid, subGuid, itemPosition, quaternion, camera.position, camera.rotation, techType);
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
            string guid = GuidHelper.GetGuid(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(itemPosition, guid, amount);
                packetSender.Send(amountChanged);
            }
        }

        public void ConstructionComplete(GameObject gameObject)
        {
            Optional<string> newlyConstructedBaseGuid = Optional<string>.Empty();
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opConstructedBase.IsPresent())
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Get();
                newlyConstructedBaseGuid = Optional<string>.Of(GuidHelper.GetGuid(constructedBase));
            }

            Vector3 itemPosition = gameObject.transform.position;
            string guid = GuidHelper.GetGuid(gameObject);

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(itemPosition, guid, newlyConstructedBaseGuid);
            packetSender.Send(constructionCompleted);
        }

        public void DeconstructionBegin(GameObject gameObject)
        {
            Vector3 itemPosition = gameObject.transform.position;
            string guid = GuidHelper.GetGuid(gameObject);

            DeconstructionBegin deconstructionBegin = new DeconstructionBegin(itemPosition, guid);
            packetSender.Send(deconstructionBegin);
        }

        public void DeconstructionComplete(GameObject gameObject)
        {
            Vector3 itemPosition = gameObject.transform.position;
            string guid = GuidHelper.GetGuid(gameObject);

            DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(itemPosition, guid);
            packetSender.Send(deconstructionCompleted);
        }
    }
}
