using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class CellVisibilityChanged : Packet
{
    public ushort PlayerId { get; }
    public List<AbsoluteEntityCell> Added { get; }
    public List<AbsoluteEntityCell> Removed { get; }

    public CellVisibilityChanged(ushort playerId, List<AbsoluteEntityCell> added, List<AbsoluteEntityCell> removed)
    {
        PlayerId = playerId;
        Added = added;
        Removed = removed;
    }
}
