using System;
using System.Timers;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        PlayerManager playerManager;
        public EventTriggerer(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
            SetupEventTimers();
        }

        public void SetupEventTimers()
        {
            // eventually this should be on a better timer so it can be saved, paused, etc
            Log.Debug("Event Triggerer started!");
            double auroraTimer = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            CreateTimer(auroraTimer * 0.2d, StoryEventType.PDA, "Story_AuroraWarning1");
            CreateTimer(auroraTimer * 0.5d, StoryEventType.PDA, "Story_AuroraWarning2");
            CreateTimer(auroraTimer * 0.8d, StoryEventType.PDA, "Story_AuroraWarning3");
            CreateTimer(auroraTimer, StoryEventType.PDA, "Story_AuroraWarning4");
            CreateTimer(auroraTimer + 24000, StoryEventType.EXTRA, "Story_AuroraExplosion");
        }

        public Timer CreateTimer(double time, StoryEventType eventType, string key)
        {
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
    }
}
