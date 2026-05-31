using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MutePlayer : Packet
{
    public SessionId SessionId;
    public bool Muted;

    public MutePlayer(SessionId sessionId, bool muted)
    {
        SessionId = sessionId;
        Muted = muted;
    }
}
