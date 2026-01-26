using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerHeldItemChanged : Packet
{
    public SessionId SessionId { get; }
    public NitroxId ItemId { get; }
    public ChangeType Type { get; }
    public NitroxTechType? IsFirstTime { get; } // If it's the first time the player used that item type it send the techType, if not null.

    public PlayerHeldItemChanged(SessionId sessionId, NitroxId itemId, ChangeType type, NitroxTechType? isFirstTime)
    {
        SessionId = sessionId;
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
