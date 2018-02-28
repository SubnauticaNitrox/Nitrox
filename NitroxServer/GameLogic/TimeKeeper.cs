using System;

namespace NitroxServer.GameLogic
{
    public class TimeKeeper
    {
        // Values taken directly from hardcoded subnautica values
        private static DateTime SUBNAUTICA_FUTURE_START_DATE = new DateTime(2287, 5, 7, 9, 36, 0);
        private static float SUBNAUTICA_BEGIN_TIME_OFFSET = 1200f * (3600f * (float)SUBNAUTICA_FUTURE_START_DATE.Hour + 60f *
                                                                             (float)SUBNAUTICA_FUTURE_START_DATE.Minute +
                                                                             (float)SUBNAUTICA_FUTURE_START_DATE.Second) / 86400f;
        private DateTime startTime = DateTime.Now;

        public float GetCurrentTime()
        {
            TimeSpan interval = DateTime.Now - startTime;
            return Convert.ToSingle(interval.TotalSeconds) + SUBNAUTICA_BEGIN_TIME_OFFSET;
        }
    }
}
