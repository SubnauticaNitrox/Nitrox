using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures.Util;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ItemPositionProcessor : ClientPacketProcessor<ItemPosition>
    {
        private const float ITEM_TRANSFORM_SMOOTH_PERIOD = 0.25f;

        public override void Process(ItemPosition drop)
        {
            Optional<GameObject> opItem = NitroxEntity.GetObjectFrom(drop.Id);

            if (opItem.HasValue)
            {
                MovementHelper.MoveRotateGameObject(opItem.Value, drop.Position.ToUnity(), drop.Rotation.ToUnity(), ITEM_TRANSFORM_SMOOTH_PERIOD);
            }
        }
    }
}
