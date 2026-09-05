using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class UnlockAchievement(string achievementId) : Packet
{
    public string AchievementId { get; } = achievementId;
}
