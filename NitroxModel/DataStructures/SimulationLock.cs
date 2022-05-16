using System;

namespace NitroxModel.DataStructures;

[Serializable]
public enum SimulationLockType
{
    /// <summary>
    ///     Exclusive locks: These are typically requested by a player to hold exclusive, unbreaking control
    ///     over the simulation of an entity. An example of this could be a player piloting
    ///     the cyclops. These can either be removed or downgraded when no longer needed.
    /// </summary>
    EXCLUSIVE,

    /// <summary>
    ///     Transient locks: A lock that allows a player to control the simulation of an entity.  Other players
    ///     can steal this lock by requesting exclusive access. An example can be the following:
    ///     a player is no longer piloting a cyclops but still simulating its movement.  A second
    ///     player can request exclusive control to pilot the cyclops. Doing so will revoke the
    ///     transient lock and grant the second player an exclusive lock.
    /// </summary>
    TRANSIENT
}
