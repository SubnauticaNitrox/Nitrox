using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SignalPingPreferenceChanged : Packet
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
