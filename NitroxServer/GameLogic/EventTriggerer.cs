using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        internal readonly Dictionary<string, Timer> eventTimers = new();
        internal readonly Stopwatch stopWatch = new();
        private readonly PlayerManager playerManager;

        // ElapsedTime is in seconds while AuroraExplosionTime is in milliseconds (be careful when mixing them)
        public double ElapsedTime;
        public double AuroraExplosionTimeInMs;

        public EventTriggerer(PlayerManager playerManager, double elapsedTime, double? auroraExplosionTime)
        {
            this.playerManager = playerManager;
            SetupEventTimers(elapsedTime, auroraExplosionTime);
        }

        private void SetupEventTimers(double elapsedTime, double? auroraExplosionTime)
        {
            // eventually this should be on a better timer so it can be saved, paused, etc
            Log.Debug($"Event Triggerer started! ElapsedTime={elapsedTime}");

            ElapsedTime = elapsedTime;
            if (auroraExplosionTime.HasValue)
            {
                AuroraExplosionTimeInMs = auroraExplosionTime.Value;
            }
            else
            {
                AuroraExplosionTimeInMs = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            }

            double elapsedTimeMilliseconds = ElapsedTime * 1000;
            CreateTimer(AuroraExplosionTimeInMs * 0.2d - elapsedTimeMilliseconds, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateTimer(AuroraExplosionTimeInMs * 0.5d - elapsedTimeMilliseconds, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateTimer(AuroraExplosionTimeInMs * 0.8d - elapsedTimeMilliseconds, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateTimer(AuroraExplosionTimeInMs - elapsedTimeMilliseconds, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateTimer(AuroraExplosionTimeInMs - elapsedTimeMilliseconds, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");
            //like the timers, except we can see how much time has passed

            stopWatch.Start();
        }

        private Timer CreateTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            //if timeOffset goes past the time
            if (time <= 0)
            {
                return null;
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

            if (!eventTimers.ContainsKey(key))
            {
                eventTimers.Add(key, timer);
            }
            return timer;
        }

        private double RandomNumber(double min, double max)
        {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        public double GetRealElapsedTime()
        {
            if (stopWatch == null)
            {
                return ElapsedTime;
            }
            // ElapsedMilliseconds and seconds should not be added without thinking of time units
            // It should be by dividing ElapsedMilliseconds by 1000
            return stopWatch.ElapsedMilliseconds * 0.001 + ElapsedTime;
        }

        public void StartWorldTime()
        {
            stopWatch.Start();
        }

        public void PauseWorldTime()
        {
            stopWatch.Stop();
        }

        public void ResetWorldTime()
        {
            stopWatch.Reset();
        }

        public void StartEventTimers()
        {
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Start();
            }
        }

        public void PauseEventTimers()
        {
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Stop();
            }
        }

        public void SendCurrentTimePacket(bool initialSync)
        {
            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedTime, initialSync));
        }

        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTime += 1200.0 - ElapsedTime % 1200.0 + 600.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTime += 1200.0 - ElapsedTime % 1200.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTime += 600.0 - ElapsedTime % 600.0;
                    break;
            }

            SendCurrentTimePacket(false);
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
