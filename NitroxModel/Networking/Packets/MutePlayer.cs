using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record MutePlayer : Packet
{
    public SessionId PlayerId;
    public bool Muted;

    public MutePlayer(SessionId playerId, bool muted)
    {
        PlayerId = playerId;
        Muted = muted;
    }
}
