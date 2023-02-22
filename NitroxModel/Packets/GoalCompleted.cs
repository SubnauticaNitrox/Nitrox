using System;

namespace NitroxModel.Packets;

[Serializable]
public class GoalCompleted : Packet
{
    public string CompletedGoal { get; }
    public float CompletionTime { get; }

    public GoalCompleted(string completedGoal, float completionTime)
    {
        CompletedGoal = completedGoal;
        CompletionTime = completionTime;
    }
}
