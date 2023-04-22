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

public static class NitroxConnectionStateExtensions
{
    public static NitroxConnectionState ToNitrox(this ConnectionState connectionState)
    {
        if ((connectionState & ConnectionState.Connected) == ConnectionState.Connected)
        {
            return NitroxConnectionState.Connected;
        }

        if ((connectionState & ConnectionState.Disconnected) == ConnectionState.Disconnected)
        {
            return NitroxConnectionState.Disconnected;
        }

        return NitroxConnectionState.Unknown;
    }
}
