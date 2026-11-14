using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing
/// </summary>
[Serializable]
public sealed class FireDoused : Packet
{
    public NitroxId Id { get; }
    public float Health { get; }
    public SessionId? SessionId { get; }
    public float DouseRate { get; }
    public bool OneShot { get; }

    /// <param name="id">The Fire id</param>
    /// <param name="health">The current health of the fire. If zero, the fire was extinguished.</param>
    /// <param name="sessionId">The player's session id. Used to differentiate multiple players dousing the same fire.</param>
    /// <param name="douseRate">The current decrease in fire health per second from this client.</param>
    /// <param name="oneShot">If set, treat as a one-time health update and ignore the douse rate.</param>
    public FireDoused(NitroxId id, float health, SessionId? sessionId, float douseRate, bool oneShot)
    {
        Id = id;
        Health = health;
        SessionId = sessionId;
        DouseRate = douseRate;
        OneShot = oneShot;
    }
}
