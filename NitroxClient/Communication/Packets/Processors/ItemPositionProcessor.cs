using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ItemPositionProcessor : GenericPacketProcessor<ItemPosition>
    {
        private const float ITEM_TRANSFORM_SMOOTH_PERIOD = 0.25f;

        private MultiplayerObjectManager multiplayerObjectManager;

        public ItemPositionProcessor(MultiplayerObjectManager multiplayerObjectManager)
        {
            this.multiplayerObjectManager = multiplayerObjectManager;
        }

        public override void Process(ItemPosition drop)
        {
            Optional<GameObject> opItem = multiplayerObjectManager.GetManagedObject(drop.Guid);

            if(opItem.IsPresent())
            {
                MovementHelper.MoveGameObject(opItem.Get(), ApiHelper.Vector3(drop.Position), ApiHelper.Quaternion(drop.Rotation), ITEM_TRANSFORM_SMOOTH_PERIOD);
            }
        }
    }
}
