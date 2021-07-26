using System;

namespace NitroxModel.DataStructures.GameLogic
{
    /**
     * These represent abstractions within the subnautica codebase.  For now, we
     * treat all of them the same within our code and demultiplex them when invoking 
     * methods.
     */
    [Serializable]
    public enum PowerType
    {
        ENERGY_INTERFACE,
        POWER_RELAY,
        POWER_SOURCE
    }
}
