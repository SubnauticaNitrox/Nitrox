using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerCinematicControllerCall : Packet
{
    public SessionId SessionId { get; }
    public NitroxId ControllerID { get; }
    public int ControllerNameHash { get; }
    public string Key { get; }
    public bool StartPlaying { get; }

    public PlayerCinematicControllerCall(SessionId sessionId, NitroxId controllerID, int controllerNameHash, string key, bool startPlaying)
    {
        SessionId = sessionId;
        ControllerID = controllerID;
        ControllerNameHash = controllerNameHash;
        Key = key;
        StartPlaying = startPlaying;
    }
}
