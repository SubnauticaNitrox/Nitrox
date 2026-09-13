using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerHeldItemChanged(SessionId sessionId, NitroxId itemId, PlayerHeldItemChanged.ChangeType type, NitroxTechType? isFirstTime)
    : Packet
{
    public enum ChangeType
    {
        DRAW_AS_TOOL,
        DRAW_AS_ITEM,
        HOLSTER_AS_TOOL,
        HOLSTER_AS_ITEM
    }

    public SessionId SessionId { get; } = sessionId;
    public NitroxId ItemId { get; } = itemId;
    public ChangeType Type { get; } = type;

    /// <summary>
    ///     True if it's the first time the player used that item type it sent the techType, if not null.
    /// </summary>
    public NitroxTechType? IsFirstTime { get; } = isFirstTime;
}
