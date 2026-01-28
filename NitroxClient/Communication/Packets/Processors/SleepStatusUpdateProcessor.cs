using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SleepStatusUpdateProcessor(SleepManager sleepManager) : IClientPacketProcessor<SleepStatusUpdate>
{
    private readonly SleepManager sleepManager = sleepManager;

    public Task Process(ClientProcessorContext context, SleepStatusUpdate packet)
    {
        if (packet.PlayersInBed > 0)
        {
            Log.InGame(Language.main.Get("Nitrox_SleepingPlayers").Replace("{SLEEPING}", packet.PlayersInBed.ToString()).Replace("{TOTAL}", packet.TotalPlayers.ToString()));
        }

        if (packet.AllPlayersInBed)
        {
            sleepManager.OnAllPlayersSleeping();
        }
        return Task.CompletedTask;
    }
}
