using System;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CellVisibilityChanged : Packet
{
    public SessionId SessionId { get; }
    public List<AbsoluteEntityCell> Added { get; }
    public List<AbsoluteEntityCell> Removed { get; }

    public CellVisibilityChanged(SessionId sessionId, List<AbsoluteEntityCell> added, List<AbsoluteEntityCell> removed)
    {
        SessionId = sessionId;
        Added = added;
        Removed = removed;
    }
}
