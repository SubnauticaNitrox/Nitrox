using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlaySunbeamEventProcessor : IClientPacketProcessor<PlaySunbeamEvent>
{
    public Task Process(ClientProcessorContext context, PlaySunbeamEvent packet)
    {
        // TODO: Look into compound goals and OnUnlock goals to bring back the necessary ones
        int beginIndex = PlaySunbeamEvent.SunbeamGoals.GetIndex(packet.EventKey);
        if (beginIndex == -1)
        {
            Log.Error($"Couldn't find the corresponding sunbeam event in {nameof(PlaySunbeamEvent.SunbeamGoals)} for key {packet.EventKey}");
            return Task.CompletedTask;
        }
        for (int i = beginIndex; i < PlaySunbeamEvent.SunbeamGoals.Length; i++)
        {
            StoryGoalManager.main.completedGoals.Remove(PlaySunbeamEvent.SunbeamGoals[i]);
        }
        // Same execution as for StoryGoalCustomEventHandler commands
        StoryGoalManager.main.OnGoalComplete(packet.EventKey);
        return Task.CompletedTask;
    }
}
