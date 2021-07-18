using System;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class TimeKeeper
    {
        public readonly double SUBNAUTICA_BEGIN_TIME_OFFSET;
        public readonly DateTime SUBNAUTICA_DATE_ORIGIN;

        public DateTime ServerStartTime { get; set; }

        private double correctionValue;

        public TimeKeeper()
        {
            ServerStartTime = DateTime.UtcNow;

            //Hardcoded value from Subnautica
            SUBNAUTICA_DATE_ORIGIN = new(2287, 5, 7, 9, 36, 0);
            SUBNAUTICA_BEGIN_TIME_OFFSET = 1200f * (3600f * SUBNAUTICA_DATE_ORIGIN.Hour
                                                    + 60f * SUBNAUTICA_DATE_ORIGIN.Minute
                                                    + SUBNAUTICA_DATE_ORIGIN.Second) / 86400f;
        }

        public void SetDay()
        {
            correctionValue += 1200.0 - (CurrentTime % 1200.0) + 600.0;
            SendCurrentTimePacket();
        }

        public void SetNight()
        {
            correctionValue += 1200.0 - (CurrentTime % 1200.0);
            SendCurrentTimePacket();
        }

        public void SkipTime()
        {
            correctionValue += 600.0 - (CurrentTime % 600.0);
            SendCurrentTimePacket();
        }

        public void SendCurrentTimePacket()
        {
            PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            playerManager.SendPacketToAllPlayers(new TimeChange(CurrentTime));
        }

        private double CurrentTime
        {
            get
            {
                TimeSpan interval = DateTime.UtcNow - ServerStartTime;
                return SUBNAUTICA_BEGIN_TIME_OFFSET + interval.TotalSeconds + correctionValue;
            }
        }
    }
}
