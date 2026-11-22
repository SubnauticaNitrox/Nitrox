using System;
using System.Collections.Generic;
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
    public Dictionary<string, bool> AnimationParameters { get; }

    public PlayerCinematicControllerCall(SessionId sessionId, NitroxId controllerID, int controllerNameHash, string key, bool startPlaying, Dictionary<string, bool> animationParameters = null)
    {
        SessionId = sessionId;
        ControllerID = controllerID;
        ControllerNameHash = controllerNameHash;
        Key = key;
        StartPlaying = startPlaying;
        AnimationParameters = animationParameters ?? new Dictionary<string, bool>();
    }
}
