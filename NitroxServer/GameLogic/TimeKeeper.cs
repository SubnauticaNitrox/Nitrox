using System;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    // Deprecated, replaced by ScheduleKeeper
    public class TimeKeeper
    {
        public readonly double SubnauticaBeginTimeOffset;
        public readonly DateTime SubnauticaDateOrigin;

        public DateTime ServerStartTime { get; set; }

        private double correctionValue;

        public TimeKeeper()
        {
            ServerStartTime = DateTime.UtcNow;

            //Hardcoded value from Subnautica
            SubnauticaDateOrigin = new(2287, 5, 7, 9, 36, 0);
            SubnauticaBeginTimeOffset = 1200f * (3600f * SubnauticaDateOrigin.Hour
                                                    + 60f * SubnauticaDateOrigin.Minute
                                                    + SubnauticaDateOrigin.Second) / 86400f;
        }

        public void SetDay()
        {
            correctionValue += 1200.0 - CurrentTime % 1200.0 + 600.0;
            SendCurrentTimePacket();
        }

        public void SetNight()
        {
            correctionValue += 1200.0 - CurrentTime % 1200.0;
            SendCurrentTimePacket();
        }

        public void SkipTime()
        {
            correctionValue += 600.0 - CurrentTime % 600.0;
            SendCurrentTimePacket();
        }

        public void SendCurrentTimePacket()
        {
            PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            playerManager.SendPacketToAllPlayers(new TimeChange(CurrentTime, false));
        }

        private double CurrentTime => SubnauticaBeginTimeOffset + (DateTime.UtcNow - ServerStartTime).TotalSeconds + correctionValue;
    }
}
