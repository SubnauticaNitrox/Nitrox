using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record Schedule(float TimeExecute, string Key, Schedule.GoalCategory Category) : Packet
{
    /// <summary>
    ///     Same as GoalType in Subnautica.
    /// </summary>
    public enum GoalCategory
    {
        PDA,
        Radio,
        Encyclopedia,
        Story
    }

    public float TimeExecute { get; } = TimeExecute;
    public string Key { get; } = Key;
    public GoalCategory Category { get; } = Category;
}
