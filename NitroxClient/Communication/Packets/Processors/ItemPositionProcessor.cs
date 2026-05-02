using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal class ItemPositionProcessor : IClientPacketProcessor<ItemPosition>
{
    private const float ITEM_TRANSFORM_SMOOTH_PERIOD = 0.25f;

    public Task Process(ClientProcessorContext context, ItemPosition drop)
    {
        Optional<GameObject> opItem = NitroxEntity.GetObjectFrom(drop.Id);

        if (opItem.HasValue)
        {
            MovementHelper.MoveRotateGameObject(opItem.Value, drop.Position.ToUnity(), drop.Rotation.ToUnity(), ITEM_TRANSFORM_SMOOTH_PERIOD);
        }
        return Task.CompletedTask;
    }
}
