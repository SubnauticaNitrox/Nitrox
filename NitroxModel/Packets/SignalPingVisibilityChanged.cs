using System;

namespace NitroxModel.Packets;

[Serializable]
public class SignalPingPreferenceChanged : Packet
{
    public string PingKey { get; }
    public bool Visible { get; }
    public int Color { get; }

    public SignalPingPreferenceChanged(string pingKey, bool visible, int color)
    {
        PingKey = pingKey;
        Visible = visible;
        Color = color;
    }
}
