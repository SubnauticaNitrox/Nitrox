using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;

namespace NitroxServer.GameLogic
{
    [TestClass]
    public class EventTriggererTest
    {
        private readonly string seed = StringHelper.GenerateRandomString(10);

        [TestMethod]
        public void VerifyEventsOrder()
        {
            EventTriggerer eventTriggerer = new(null, null, null, seed, 0.0, null, null);
            List<string> eventsOrder = new() { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };
            List<string> eventTriggererEvents = new(eventTriggerer.eventTimers.Keys);

            Assert.AreEqual(eventsOrder.Count, eventTriggererEvents.Count, "eventsOrder and eventTriggererEvents are not the same size.");
            for (int i = 0; i < eventsOrder.Count; i++)
            {
                Assert.AreEqual(eventsOrder[i], eventTriggererEvents[i], $"The {i} item of eventsOrder was not equals to eventTriggererEvents");
            }
        }

        [TestMethod]
        public void AuroraExplosionAndWarningTime()
        {
            EventTriggerer eventTriggerer = new(null, null, null, seed, 0.0, TimeSpan.FromMinutes(40).TotalMilliseconds, null);
            Assert.AreEqual(eventTriggerer.eventTimers["Story_AuroraWarning4"].Interval, eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval);
        }

        [TestMethod]
        public void TestTimeSkip()
        {
            EventTriggerer eventTriggerer = new(null, null, null, seed, 480.0, null, null);
            eventTriggerer.PauseWorld();

            double interval = eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval;
            eventTriggerer.ElapsedTimeMs += TimeSpan.FromMinutes(40).TotalMilliseconds;
            Assert.AreEqual(interval - TimeSpan.FromMinutes(40).TotalMilliseconds, eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval);

            interval = eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval;
            eventTriggerer.ElapsedTimeMs += TimeSpan.FromMinutes(2).TotalMilliseconds;
            Assert.AreEqual(interval - TimeSpan.FromMinutes(2).TotalMilliseconds, eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval);
        }

        [TestMethod]
        public void TestAuroraEventsCalculations()
        {
            EventTriggerer eventTriggerer = new(null, null, null, seed, 890000, 2400000, 480000);
            eventTriggerer.PauseWorld();

            Assert.AreEqual(4, eventTriggerer.eventTimers.Count);

            eventTriggerer = new(null, null, null, seed, 1500000, 2400000, 480000);
            eventTriggerer.PauseWorld();

            Assert.AreEqual(3, eventTriggerer.eventTimers.Count);

            eventTriggerer = new(null, null, null, seed, 2016000, 2400000, 480000);
            eventTriggerer.PauseWorld();

            Assert.AreEqual(2, eventTriggerer.eventTimers.Count);
        }
    }
}
