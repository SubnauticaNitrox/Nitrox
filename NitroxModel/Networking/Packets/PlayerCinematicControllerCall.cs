using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PlayerCinematicControllerCall : Packet
{
    public ushort PlayerId { get; }
    public NitroxId ControllerID { get; }
    public int ControllerNameHash { get; }
    public string Key { get; }
    public bool StartPlaying { get; }

    public PlayerCinematicControllerCall(ushort playerId, NitroxId controllerID, int controllerNameHash, string key, bool startPlaying)
    {
        PlayerId = playerId;
        ControllerID = controllerID;
        ControllerNameHash = controllerNameHash;
        Key = key;
        StartPlaying = startPlaying;
    }
}
