using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        internal readonly Dictionary<string, Timer> eventTimers = new();
        private readonly Stopwatch stopWatch = new();
        private readonly PlayerManager playerManager;

        public readonly double AuroraExplosionTime;

        private double elapsedTimeOutsideStopWatch;

        public double ElapsedTime
        {
            get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatch;
            private set => elapsedTimeOutsideStopWatch = value - stopWatch.ElapsedMilliseconds;
        }

        public double ElapsedSeconds
        {
            get => ElapsedTime * 0.001;
            private set => ElapsedTime = value * 1000;
        }

        public EventTriggerer(PlayerManager playerManager, double elapsedTime, double? auroraExplosionTime)
        {
            this.playerManager = playerManager;
            // Default time in Base SN is 480s
            elapsedTimeOutsideStopWatch = elapsedTime == 0 ? TimeSpan.FromMinutes(8).TotalMilliseconds : elapsedTime;

            Log.Debug($"Event Triggerer started! ElapsedTime={Math.Floor(ElapsedSeconds)}s");


            if (auroraExplosionTime.HasValue)
            {
                AuroraExplosionTime = auroraExplosionTime.Value;
            }
            else
            {
                AuroraExplosionTime = elapsedTimeOutsideStopWatch + RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            }

            CreateTimer(AuroraExplosionTime * 0.2d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateTimer(AuroraExplosionTime * 0.5d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateTimer(AuroraExplosionTime * 0.8d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateTimer(AuroraExplosionTime - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateTimer(AuroraExplosionTime - ElapsedTime, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");

            stopWatch.Start();
        }

        private void CreateTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            if (time <= 0) // Ignoring if time is in the past
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

        private double RandomNumber(double min, double max)
        {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
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

        public void SendCurrentTimePacket(bool initialSync, Optional<Player> player)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new TimeChange(ElapsedSeconds, initialSync));
            }
            else
            {
                playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds, initialSync));
            }
        }

        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTime += 1200000.0 - ElapsedTime % 1200000.0 + 600000.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTime += 1200000.0 - ElapsedTime % 1200000.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTime += 600000.0 - ElapsedTime % 600000.0;
                    break;
            }

            SendCurrentTimePacket(false, Optional.Empty);
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
