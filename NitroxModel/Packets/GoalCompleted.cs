using System;

namespace NitroxModel.Packets;

[Serializable]
public class GoalCompleted : Packet
{
    public string CompletedGoal { get; }

    public GoalCompleted(string completedGoal)
    {
        CompletedGoal = completedGoal;
    }
}
