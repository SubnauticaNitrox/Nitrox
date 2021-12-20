using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxServer.GameLogic
{
    [TestClass]
    public class EventTriggererTest
    {
        [TestMethod]
        public void VerifyEventsOrder()
        {
            EventTriggerer eventTriggerer = new(null, 0.0, null);
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
            EventTriggerer eventTriggerer = new(null, 0.0, 30d);
            Assert.AreEqual(eventTriggerer.eventTimers["Story_AuroraWarning4"].Interval, eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval);
        }
    }
}
