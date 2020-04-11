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
        private GameData gameData;
        private Stopwatch stopWatch;
        private double elapsedTime;
        private double auroraExplosionTime;
        public EventTriggerer(PlayerManager playerManager, GameData gameData)
        {
            this.playerManager = playerManager;
            this.gameData = gameData;
            gameData.StoryTiming.EventTriggerer = this; //bad hack, we should probably just serialize this class
            SetupEventTimers();
        }

        public void SetupEventTimers()
        {
            // eventually this should be on a better timer so it can be saved, paused, etc
            Log.Debug("Event Triggerer started!");

            StoryTimingData storyTiming = gameData.StoryTiming;
            elapsedTime = storyTiming.ElapsedTime;
            if (storyTiming.AuroraExplosionTime.HasValue)
            {
                auroraExplosionTime = storyTiming.AuroraExplosionTime.Value;
            }
            else
            {
                auroraExplosionTime = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            }

            CreateTimer(auroraExplosionTime * 0.2d - elapsedTime, StoryEventType.PDA, "Story_AuroraWarning1");
            CreateTimer(auroraExplosionTime * 0.5d - elapsedTime, StoryEventType.PDA, "Story_AuroraWarning2");
            CreateTimer(auroraExplosionTime * 0.8d - elapsedTime, StoryEventType.PDA, "Story_AuroraWarning3");
            CreateTimer(auroraExplosionTime - elapsedTime, StoryEventType.PDA, "Story_AuroraWarning4");
            CreateTimer(auroraExplosionTime + 24000 - elapsedTime, StoryEventType.EXTRA, "Story_AuroraExplosion");
            //like the timers, except we can see how much time has passed
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public Timer CreateTimer(double time, StoryEventType eventType, string key)
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

        public double RandomNumber(double min, double max)
        {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        public void Serialize()
        {
            gameData.StoryTiming.ElapsedTime = elapsedTime + stopWatch.ElapsedMilliseconds;
            gameData.StoryTiming.AuroraExplosionTime = auroraExplosionTime;
            Log.Debug("Event Triggerer details:");
            Log.Debug($"  Time: {gameData.StoryTiming.ElapsedTime}");
            Log.Debug($"  Explode: {gameData.StoryTiming.AuroraExplosionTime}");
        }
    }
}
