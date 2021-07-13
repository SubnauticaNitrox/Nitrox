using System;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class TimeKeeper
    {
        public readonly float SUBNAUTICA_BEGIN_TIME_OFFSET;
        public readonly DateTime SUBNAUTICA_DATE_ORIGIN;
        public DateTime ServerStartTime { get; set; }

        private float correctionValue;

        public TimeKeeper()
        {
            ServerStartTime = DateTime.UtcNow;
            SUBNAUTICA_DATE_ORIGIN = new(2287, 5, 7, 9, 36, 0);
            SUBNAUTICA_BEGIN_TIME_OFFSET = 1200f * (3600f * SUBNAUTICA_DATE_ORIGIN.Hour
                                                    + 60f * SUBNAUTICA_DATE_ORIGIN.Minute
                                                    + SUBNAUTICA_DATE_ORIGIN.Second) / 86400f;
        }

        public void SetDay()
        {
            correctionValue += 1200.0f - CurrentTime % 1200.0f + 600.0f;
            SendCurrentTimePacket();
        }

        public void SetNight()
        {
            correctionValue += 1200.0f - CurrentTime % 1200.0f;
            SendCurrentTimePacket();
        }

        public void SkipTime()
        {
            correctionValue += 600.0f - CurrentTime % 600.0f;
            SendCurrentTimePacket();
        }

        // Convenience for sending the TimeChange packet to a player or to all online players
        public void SendCurrentTimePacket(Player player = null)
        {
            if (player != null)
            {
                player.SendPacket(new TimeChange(CurrentTime));
            }
            else
            {
                PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                playerManager.SendPacketToAllPlayers(new TimeChange(CurrentTime));
            }
        }

        private float CurrentTime
        {
            get
            {
                TimeSpan interval = DateTime.UtcNow - ServerStartTime;
                return SUBNAUTICA_BEGIN_TIME_OFFSET + Convert.ToSingle(interval.TotalSeconds) + correctionValue;
            }
        }
    }
}
