using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerHeldItemChanged : Packet
    {
        public ushort PlayerId { get; }
        public NitroxId ItemId { get; }
        public ChangeType Type { get; }
        public NitroxTechType IsFirstTime { get; } // If it's the first time the player used that item type it send the techType, if not null.

        public PlayerHeldItemChanged(ushort playerId, NitroxId itemId, ChangeType type, NitroxTechType isFirstTime)
        {
            PlayerId = playerId;
            ItemId = itemId;
            Type = type;
            IsFirstTime = isFirstTime;
        }

        public enum ChangeType
        {
            DRAW_AS_TOOL,
            DRAW_AS_ITEM,
            HOLSTER_AS_TOOL,
            HOLSTER_AS_ITEM
        }
    }
}
