using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record GoalCompleted : Packet
{
    public string CompletedGoal { get; }
    public float CompletionTime { get; }

    public GoalCompleted(string completedGoal, float completionTime)
    {
        CompletedGoal = completedGoal;
        CompletionTime = completionTime;
    }
}
