using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using NitroxModel.Packets;
using NitroxServer.Helper;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        internal readonly Dictionary<string, Timer> eventTimers = new();
        private readonly Stopwatch stopWatch = new();
        private readonly PlayerManager playerManager;

        public readonly double AuroraExplosionTimeMs;

        private double elapsedTimeOutsideStopWatchMs;

        public double ElapsedTimeMs
        {
            get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatchMs;
            internal set
            {
                foreach (Timer timer in eventTimers.Values)
                {
                    timer.Interval = Math.Max(1, timer.Interval - (value - ElapsedTimeMs));
                }
                elapsedTimeOutsideStopWatchMs = value - stopWatch.ElapsedMilliseconds;
            }
        }

        public double ElapsedSeconds
        {
            get => ElapsedTimeMs * 0.001;
            private set => ElapsedTimeMs = value * 1000;
        }

        // Using ceiling because days count start at 1 and not 0
        public int Day => (int)Math.Ceiling(ElapsedTimeMs / TimeSpan.FromMinutes(20).TotalMilliseconds);

        public EventTriggerer(PlayerManager playerManager, string seed, double elapsedTime, double? auroraExplosionTime)
        {
            this.playerManager = playerManager;
            // Default time in Base SN is 480s
            elapsedTimeOutsideStopWatchMs = elapsedTime == 0 ? TimeSpan.FromSeconds(480).TotalMilliseconds : elapsedTime;
            AuroraExplosionTimeMs = auroraExplosionTime ?? GenerateDeterministicAuroraTime(seed);

            CreateTimer(AuroraExplosionTimeMs * 0.2d - ElapsedTimeMs, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateTimer(AuroraExplosionTimeMs * 0.5d - ElapsedTimeMs, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateTimer(AuroraExplosionTimeMs * 0.8d - ElapsedTimeMs, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateTimer(AuroraExplosionTimeMs - ElapsedTimeMs, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateTimer(AuroraExplosionTimeMs - ElapsedTimeMs, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");

            stopWatch.Start();
            Log.Debug($"Event Triggerer started! ElapsedTime={Math.Floor(ElapsedSeconds)}s");
        }

        /// <summary>
        /// When starting the server, if some events already happened, the time parameter will be &lt; 0
        /// in which case we don't want to create the timer
        /// </summary>
        /// <param name="time">In milliseconds</param>
        private void CreateTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            if (time <= 0)
            {
                return;
            }

            Timer timer = new()
            {
                Interval = time,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += delegate
            {
                eventTimers.Remove(key);
                Log.Info($"Triggering event type {eventType} at time {time} with param {key}");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };

            eventTimers.Add(key, timer);
        }

        //Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        private double GenerateDeterministicAuroraTime(string seed)
        {
            DeterministicGenerator generator = new(seed, nameof(EventTriggerer));
            return elapsedTimeOutsideStopWatchMs + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
        }

        public void StartWorld()
        {
            stopWatch.Start();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Start();
            }
        }

        public void PauseWorld()
        {
            stopWatch.Stop();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Stop();
            }
        }

        internal void ResetWorld()
        {
            stopWatch.Reset();
        }

        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0 + 600000.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTimeMs += 600000.0 - ElapsedTimeMs % 600000.0;
                    break;
            }

            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds, false));
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
