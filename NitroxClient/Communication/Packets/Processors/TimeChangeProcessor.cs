using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class TimeChangeProcessor : ClientPacketProcessor<TimeChange>
    {
        public override void Process(TimeChange timeChangePacket)
        {
            double oldTimePassedAsDouble = DayNightCycle.main.timePassedAsDouble;
            double newTimePassedAsDouble = timeChangePacket.CurrentTime;
            DayNightCycle.main.timePassedAsDouble = newTimePassedAsDouble;
            DayNightCycle.main.StopSkipTimeMode();

            if (timeChangePacket.InitialSync)
            {
                AuroraWarnings auroraWarnings = GameObject.Find("Player/SpawnPlayerSounds/PlayerSounds(Clone)/auroraWarnings").GetComponent<AuroraWarnings>();

                Utils.ScalarMonitor auroraTimeMonitor = auroraWarnings.timeMonitor;
                auroraTimeMonitor.Init((float)newTimePassedAsDouble);

                Utils.ScalarMonitor crashedTimeMonitor = CrashedShipExploder.main.timeMonitor;
                crashedTimeMonitor.Init((float)newTimePassedAsDouble);
            }

            Log.Info($"Processed a Time Change [from {oldTimePassedAsDouble} to {DayNightCycle.main.timePassedAsDouble}]");
        }
    }
}
