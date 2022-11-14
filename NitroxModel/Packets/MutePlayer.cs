using System;

namespace NitroxModel.Packets;

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
