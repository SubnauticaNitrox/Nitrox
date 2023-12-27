using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities;

public interface ISimulationWhitelist
{
    /// <summary>
    ///     We don't want to give out simulation to all entities that the server sent out because there is a lot of stationary items and junk (TechType.None).
    ///     It is easier to maintain a list of items we should simulate than try to blacklist items. This list should not be checked for non-server spawned items
    ///     as they were probably dropped by the player and are mostly guaranteed to move.
    /// </summary>
    HashSet<NitroxTechType> MovementWhitelist { get; }

    /// <summary>
    ///     We differentiate the entities which should be simulated because of one of their behaviour (ie for utility)
    ///     from those are simulated for their movements.
    /// </summary>
    HashSet<NitroxTechType> UtilityWhitelist { get; }
}
