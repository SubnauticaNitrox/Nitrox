using System.Collections.Generic;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    public class StoryGoalWhitelist
    {
        /*
         * We don't wanna have local duplicate when we're simulating it server side like aurora timers
         */
        public static readonly HashSet<string> SimulatedByServer = new()
        {
            "Story_AuroraWarning1",
            "Story_AuroraWarning2",
            "Story_AuroraWarning3",
            "Story_AuroraWarning4",
            "Story_AuroraExplosion"
        };
    }
}
