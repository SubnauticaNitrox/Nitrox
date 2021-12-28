using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerHeldItemChanged : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual NitroxId ItemId { get; protected set; }
        [Index(2)]
        public virtual ChangeType Type { get; protected set; }
        [Index(3)]
        public virtual NitroxTechType IsFirstTime { get; protected set; } // If it's the first time the player used that item type it send the techType, if not null.

        public PlayerHeldItemChanged() { }

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
