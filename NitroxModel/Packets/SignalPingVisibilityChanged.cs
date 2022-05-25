using System;

namespace NitroxModel.Packets;

[Serializable]
public class SignalPingPreferenceChanged : Packet
{
    public string PingKey { get; }

    public bool? Visible { get; }
    public int? Color { get; }

    public SignalPingPreferenceChanged(string pingKey, bool visible)
    {
        PingKey = pingKey;
        Visible = visible;
    }

    public SignalPingPreferenceChanged(string pingKey, int color)
    {
        PingKey = pingKey;
        Color = color;
    }
}
