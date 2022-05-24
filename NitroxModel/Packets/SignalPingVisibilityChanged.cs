using System;

namespace NitroxModel.Packets;

[Serializable]
public class SignalPingVisibilityChanged : Packet
{
    public string PingKey { get; }
    public bool Visible { get; }

    public SignalPingVisibilityChanged(string pingKey, bool visible)
    {
        PingKey = pingKey;
        Visible = visible;
    }
}
