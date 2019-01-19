using System;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class TimeKeeper
    {
        // Values taken directly from hardcoded subnautica values
        private static DateTime SUBNAUTICA_FUTURE_START_DATE = new DateTime(2287, 5, 7, 9, 36, 0);

        private static float SUBNAUTICA_BEGIN_TIME_OFFSET = 1200f / 86400f *
                                                            (3600f * (float)SUBNAUTICA_FUTURE_START_DATE.Hour +
                                                             60f * (float)SUBNAUTICA_FUTURE_START_DATE.Minute +
                                                             (float)SUBNAUTICA_FUTURE_START_DATE.Second);

        public DateTime ServerStartTime { get; set; } = DateTime.Now;

        // Discrepancy value for player based time modifications
        private float correctionValue;

        public void SetDay()
        {
            correctionValue += 1200.0f - GetCurrentTime() % 1200.0f + 600.0f;
            SendCurrentTimePacket();
        }

        public void SetNight()
        {
            correctionValue += 1200.0f - GetCurrentTime() % 1200.0f;
            SendCurrentTimePacket();
        }

        // Convenience for sending the TimeChange packet to a player or to all online players
        public void SendCurrentTimePacket(Player player = null)
        {
            if (player != null)
            {
                player.SendPacket(new TimeChange(GetCurrentTime()));
            }
            else
            {
                PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                playerManager.SendPacketToAllPlayers(new TimeChange(GetCurrentTime()));
            }
        }

        private float GetCurrentTime()
        {
            TimeSpan interval = DateTime.Now - ServerStartTime;
            return Convert.ToSingle(interval.TotalSeconds) + SUBNAUTICA_BEGIN_TIME_OFFSET + correctionValue;
        }
    }
}
