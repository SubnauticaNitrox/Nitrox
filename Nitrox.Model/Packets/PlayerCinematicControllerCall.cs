using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class PlayerCinematicControllerCall : Packet
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
