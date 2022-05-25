using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialPingInstancePreferences
    {
        public HashSet<string> HiddenSignalPings { get; set; }
        public List<KeyValuePair<string, int>> ColorPreferences { get; set; }

        protected InitialPingInstancePreferences()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialPingInstancePreferences(HashSet<string> hiddenSignalPings, List<KeyValuePair<string, int>> colorPreferences)
        {
            HiddenSignalPings = hiddenSignalPings;
            ColorPreferences = colorPreferences;
        }
    }
}
