using System;
using System.Diagnostics;
using System.Timers;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        private PlayerManager playerManager;
        private Stopwatch stopWatch;
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
                Log.Info("Triggering event type " + eventType.ToString() + " at time " + time.ToString() + " with param " + key.ToString());
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };
            timer.Interval = time;
            timer.Enabled = true;
            timer.AutoReset = false;
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
    }
}
