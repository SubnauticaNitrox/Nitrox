using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SleepStatusUpdateProcessor : ClientPacketProcessor<SleepStatusUpdate>
{
    private readonly SleepManager sleepManager;

    public SleepStatusUpdateProcessor(SleepManager sleepManager)
    {
        this.sleepManager = sleepManager;
    }

    public override void Process(SleepStatusUpdate packet)
    {
        if (packet.WasCancelled)
        {
            sleepManager.OnSleepCancelled();
            Log.InGame(Language.main.Get("Nitrox_SleepCancelled"));
            return;
        }

        if (packet.SleepingPlayers > 0)
        {
            Log.InGame(Language.main.Get("Nitrox_SleepingPlayers").Replace("{SLEEPING}", packet.SleepingPlayers.ToString()).Replace("{TOTAL}", packet.TotalPlayers.ToString()));
        }

        if (packet.AllPlayersSleeping)
        {
            sleepManager.OnAllPlayersSleeping();
        }
    }
}
