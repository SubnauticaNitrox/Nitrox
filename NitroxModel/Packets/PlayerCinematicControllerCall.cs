using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerCinematicControllerCall : Packet
{
    public ushort PlayerId { get; set; }
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
