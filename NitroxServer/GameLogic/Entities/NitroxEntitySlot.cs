using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer.GameLogic.Entities
{
    public class NitroxEntitySlot
    {
        public string[] AllowedTypes;

        public string BiomeType;

        public NitroxEntitySlot(string biomeType, string[] allowedTypes)
        {
            BiomeType = biomeType;
            AllowedTypes = allowedTypes;
        }
    }
}
