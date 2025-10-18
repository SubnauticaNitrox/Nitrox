using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MutePlayer : Packet
{
    public ushort PlayerId;
    public bool Muted;

    public MutePlayer(ushort playerId, bool muted)
    {
        PlayerId = playerId;
        Muted = muted;
    }
}
