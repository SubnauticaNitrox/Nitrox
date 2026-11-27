using System;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerAnimatorStateUpdate(SessionId sessionId, Dictionary<string, bool> animatorParameters) : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public Dictionary<string, bool> AnimatorParameters { get; } = animatorParameters;
}
