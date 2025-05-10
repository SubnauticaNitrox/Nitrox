using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ItemPositionProcessor : IClientPacketProcessor<ItemPosition>
    {
        private const float ITEM_TRANSFORM_SMOOTH_PERIOD = 0.25f;

        public Task Process(IPacketProcessContext context, ItemPosition drop)
        {
            Optional<GameObject> opItem = NitroxEntity.GetObjectFrom(drop.Id);

            if (opItem.HasValue)
            {
                MovementHelper.MoveRotateGameObject(opItem.Value, drop.Position.ToUnity(), drop.Rotation.ToUnity(), ITEM_TRANSFORM_SMOOTH_PERIOD);
            }
            return Task.CompletedTask;
        }
    }
}
