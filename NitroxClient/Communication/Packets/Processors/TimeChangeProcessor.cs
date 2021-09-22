using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
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
            DayNightCycle.main.timePassedAsDouble = newTimePassedAsDouble; //TODO: account for player latency
            DayNightCycle.main.StopSkipTimeMode();

            if (timeChangePacket.InitialSync)
            {
                AuroraWarnings auroraWarnings = GameObject.Find("Player/SpawnPlayerSounds/PlayerSounds(Clone)/auroraWarnings").GetComponent<AuroraWarnings>();
                
                Utils.ScalarMonitor auroraTimeMonitor = (Utils.ScalarMonitor)typeof(AuroraWarnings).GetField("timeMonitor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(auroraWarnings);
                auroraTimeMonitor.Init((float)newTimePassedAsDouble);

                Utils.ScalarMonitor crashedTimeMonitor = (Utils.ScalarMonitor)typeof(CrashedShipExploder).GetField("timeMonitor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(CrashedShipExploder.main);
                crashedTimeMonitor.Init((float)newTimePassedAsDouble);
            }
            
            Log.Info($"Processed a Time Change [from {oldTimePassedAsDouble} to {DayNightCycle.main.timePassedAsDouble}]");
        }
    }
}
