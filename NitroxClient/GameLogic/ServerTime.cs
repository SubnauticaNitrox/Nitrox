using System;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    /// <summary>
    /// The synchronized UTC server time.
    /// </summary>
    public class ServerTime
    {
        private readonly IPacketSender packetSender;

        private DateTime serverUtcTime;
        private DateTime clientUtcTime;

        public ServerTime(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        /// <summary>
        /// Returns the current server's UTC time in relation to the client's time
        /// </summary>
        /// <returns>Current server UTC time, or <see cref="DateTime.MinValue"/> if the server time has not been synced.</returns>
        public DateTime GetCurrentServerTime()
        {
            if (serverUtcTime == null)
            {
                return DateTime.MinValue;
            }

            return new DateTime(serverUtcTime.Ticks + (DateTime.UtcNow.Ticks - clientUtcTime.Ticks));
        }

        /// <summary>
        /// Sets the server's UTC time synced with the latency.
        /// </summary>
        /// <param name="serverUtcTotalMilliseconds">The total milliseconds from <see cref="DateTime.UtcNow"/> that the server sent with the request response packet</param>
        /// <param name="packetSentTotalMilliseconds">The total milliseconds from <see cref="DateTime.UtcNow"/> that was calculated at the time the packet was
        ///     sent to the server</param>
        /// <param name="packetReceivedTotalMilliseconds">If 0, it will use the total milliseconds from <see cref="DateTime.UtcNow"/></param>
        public void SetServerUtcTime(double serverUtcTotalMilliseconds, double packetSentTotalMilliseconds, double packetReceivedTotalMilliseconds = 0)
        {
            long parsedPacketReceivedTotalMilliseconds = packetReceivedTotalMilliseconds == 0 ? DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond : (long)packetReceivedTotalMilliseconds;

            // Subtract the time the packet was sent from the current time for the round trip latency. Divide by 2 for the one way latency, then convert to ticks.
            long oneWayLatencyTicks = ((parsedPacketReceivedTotalMilliseconds - (long)packetSentTotalMilliseconds) / 2) * TimeSpan.TicksPerMillisecond;

            serverUtcTime = new DateTime(((long)serverUtcTotalMilliseconds * TimeSpan.TicksPerMillisecond) + oneWayLatencyTicks, DateTimeKind.Utc);
            clientUtcTime = new DateTime(parsedPacketReceivedTotalMilliseconds * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);

            Log.Debug("[TimeManager Set new server time." 
                + " Server time: " + serverUtcTime.ToString("yyyy-MM-dd HH:mm:ss.fff") 
                + " Client time: " + clientUtcTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                + " Roundway latency: " + Convert.ToSingle((parsedPacketReceivedTotalMilliseconds - (long)packetSentTotalMilliseconds) / 2) + "]");
        }

        /// <summary>
        /// Get a seed for a random number generator based on the server time. Rounds to the nearest 10 second point.
        /// </summary>
        /// <returns>Last 8 digits of server ticks rounded to the nearest 10 second point.</returns>
        public int GetSeedFromCurrentServerTime()
        {
            return (int)(Math.Round((decimal)(GetCurrentServerTime().Ticks / TimeSpan.TicksPerSecond) / 10) % 100000000);
        }
    }
}
