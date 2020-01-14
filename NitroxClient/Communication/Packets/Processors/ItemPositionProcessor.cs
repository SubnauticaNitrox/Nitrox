﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ItemPositionProcessor : ClientPacketProcessor<ItemPosition>
    {
        private const float ITEM_TRANSFORM_SMOOTH_PERIOD = 0.25f;

        public override void Process(ItemPosition drop)
        {
            Optional<GameObject> opItem = NitroxIdentifier.GetObjectFrom(drop.Id);

            if (opItem.IsPresent())
            {
                MovementHelper.MoveRotateGameObject(opItem.Get(), drop.Position, drop.Rotation, ITEM_TRANSFORM_SMOOTH_PERIOD);
            }
        }
    }
}
