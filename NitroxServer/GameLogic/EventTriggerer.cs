using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        private PlayerManager playerManager;
        private Stopwatch stopWatch;
        private Dictionary<String, Timer> eventTimers = new Dictionary<string, Timer>();
        public double ElapsedTime;
        public double AuroraExplosionTime;
        public EventTriggerer(PlayerManager playerManager, double elapsedTime, double? auroraExplosionTime)
        {
            this.playerManager = playerManager;
            SetupEventTimers(elapsedTime, auroraExplosionTime);
        }

        private void SetupEventTimers(double elapsedTime, double? auroraExplosionTime)
        {
            // eventually this should be on a better timer so it can be saved, paused, etc
            Log.Debug("Event Triggerer started!");

            ElapsedTime = elapsedTime;
            if (auroraExplosionTime.HasValue)
            {
                AuroraExplosionTime = auroraExplosionTime.Value;
            }
            else
            {
                AuroraExplosionTime = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            }

            CreateTimer(AuroraExplosionTime * 0.2d - ElapsedTime, StoryEventType.PDA, "Story_AuroraWarning1");
            CreateTimer(AuroraExplosionTime * 0.5d - ElapsedTime, StoryEventType.PDA, "Story_AuroraWarning2");
            CreateTimer(AuroraExplosionTime * 0.8d - ElapsedTime, StoryEventType.PDA, "Story_AuroraWarning3");
            CreateTimer(AuroraExplosionTime - ElapsedTime, StoryEventType.PDA, "Story_AuroraWarning4");
            CreateTimer(AuroraExplosionTime + 24000 - ElapsedTime, StoryEventType.EXTRA, "Story_AuroraExplosion");
            //like the timers, except we can see how much time has passed
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        private Timer CreateTimer(double time, StoryEventType eventType, string key)
        {
            //if timeOffset goes past the time
            if (time <= 0)
            {
                return null;
            }

            Timer timer = new Timer();
            timer.Elapsed += delegate
            {
                Log.Info($"Triggering event type {eventType} at time {time} with param {key}");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };
            timer.Interval = time;
            timer.Enabled = true;
            timer.AutoReset = false;
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
            return stopWatch.ElapsedMilliseconds + ElapsedTime;
        }

        public void StartWorldTime()
        {
            stopWatch.Start();
        }

        public void PauseWorldTime()
        {
            stopWatch.Stop();
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
    }
}
