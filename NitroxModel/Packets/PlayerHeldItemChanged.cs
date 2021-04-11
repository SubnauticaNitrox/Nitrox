using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerHeldItemChanged : Packet
    {
        public ushort PlayerId { get; }
        public NitroxId ItemId { get; }
        public PlayerHeldItemChangedType Type { get; }

        public PlayerHeldItemChanged(ushort playerId, NitroxId itemId, PlayerHeldItemChangedType type)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Type = type;
        }

        public override string ToString()
        {
            return $"[PlayerHeldItemChanged - PlayerId: {PlayerId}, Type: {Type}, ItemId: {ItemId}]";
        }
    }

    public enum PlayerHeldItemChangedType
    {
        DRAW_AS_TOOL,
        DRAW_AS_ITEM,
        HOLSTER_AS_TOOL,
        HOLSTER_AS_ITEM
    }
}
