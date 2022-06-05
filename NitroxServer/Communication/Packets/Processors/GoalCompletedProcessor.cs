using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class GoalCompletedProcessor : AuthenticatedPacketProcessor<GoalCompleted>
{
    public override void Process(GoalCompleted packet, Player player)
    {
        player.CompletedGoals.Add(packet.CompletedGoal);
    }
}
