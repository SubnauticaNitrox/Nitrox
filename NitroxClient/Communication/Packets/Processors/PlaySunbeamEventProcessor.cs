using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

public class PlaySunbeamEventProcessor : ClientPacketProcessor<PlaySunbeamEvent>
{
    public override void Process(PlaySunbeamEvent packet)
    {
        // TODO: Look into compound goals and OnUnlock goals to bring back the necessary ones
        for (int i = (int)packet.Event; i < PlaySunbeamEvent.SunbeamGoals.Count; i++)
        {
            StoryGoalManager.main.completedGoals.Remove(PlaySunbeamEvent.SunbeamGoals[i]);
        }
        // Same execution as for StoryGoalCustomEventHandler commands
        StoryGoalManager.main.OnGoalComplete(PlaySunbeamEvent.SunbeamGoals[(int)packet.Event]);
    }
}
