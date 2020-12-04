using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public class UntypedCellEntityBootstrapper : IEntityBootstrapper
    {
        // In the server, cell visibility logic isn't perfectly aligned with logic in the central game's 
        // ClipMapManager.  Some entities have long-range visibility but are encapsulated in cells with 
        // short-range visibility.  This is reconciled in-game with custom logic for processing cell 
        // roots.  We are looking to expand our own handling of cell entities by tracking the player 
        // position; however, as a stop gap we still want to patch entities impacted by this.
        private readonly Dictionary<NitroxId, int> levelOverridesByEntityId = new Dictionary<NitroxId, int>()
        {
            {new NitroxId("bb20a676-5523-4a53-66df-a24e0c0ef83c"), 3} // the cell root that encapsulates the gun should be forced to a higher visibility
        };

        public void Prepare(Entity entity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            if (levelOverridesByEntityId.TryGetValue(entity.Id, out int level))
            {
                entity.Level = level;
            }
        }

    }
}
