using System;
using System.Collections.Generic;
using System.Timers;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {

        PlayerManager playerManager;
        EventData eventData;

        public EventTriggerer(PlayerManager playerManager, EventData eventData)
        {
            this.playerManager = playerManager;
            this.eventData = eventData;
            if (!eventData.HasTimers())
            {
                SetupEventTimers();
            }
            eventData.StartTimers();
        }

        public void SetupEventTimers()
        {
            // eventually this should be on a better timer so it can be saved, paused, etc
            Log.Debug("Event Triggerer started!");
            double auroraTimer = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            eventData.AddTimer(CreateTimer(auroraTimer * 0.2d, StoryEventType.PDA, "Story_AuroraWarning1"));
            eventData.AddTimer(CreateTimer(auroraTimer * 0.5d, StoryEventType.PDA, "Story_AuroraWarning2"));
            eventData.AddTimer(CreateTimer(auroraTimer * 0.8d, StoryEventType.PDA, "Story_AuroraWarning3"));
            eventData.AddTimer(CreateTimer(auroraTimer, StoryEventType.PDA, "Story_AuroraWarning4"));
            eventData.AddTimer(CreateTimer(auroraTimer + 24000, StoryEventType.Extra, "Story_AuroraExplosion"));
        }

        public NitroxTimer CreateTimer(double time, StoryEventType eventType, string key)
        {
            NitroxTimer timer = new NitroxTimer();
            timer.Key = key;
            timer.Elapsed += delegate
            {
                Log.Info("Triggering event type " + eventType.ToString() + " at time " + time.ToString() + " with param " + key.ToString());
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };
            timer.Interval = time;
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
