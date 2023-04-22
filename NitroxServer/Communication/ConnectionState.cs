using LiteNetLib;

namespace NitroxServer.Communication;

public enum NitroxConnectionState
{
    Unknown,
    Disconnected,
    Connected,
    Reserved,
    InGame
}

public static class NitroxConnectionStateExtension
{
    public static NitroxConnectionState ToNitrox(this ConnectionState connectionState)
    {
        NitroxConnectionState state = NitroxConnectionState.Unknown;

        if (connectionState.HasFlag(ConnectionState.Connected))
        {
            state = NitroxConnectionState.Connected;
        }

        if (connectionState.HasFlag(ConnectionState.Disconnected))
        {
            state = NitroxConnectionState.Disconnected;
        }

        return state;
    }
}
