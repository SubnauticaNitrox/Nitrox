using System.Collections.Generic;

namespace NitroxServer.GameLogic.Entities
{
    public class SimulationWhitelist
    {
        /*
         * We don't want to give out simulation to all entities that the server sent out
         * because there is a lot of stationary items and junk (TechType.None).  It is easier
         * to maintain a list of items we should simulate than try to blacklist items.  This
         * list should not be checked for non-server spawned items as they were probably dropped
         * by the player and are mostly guaranteed to move.
         */
        public static readonly HashSet<TechType> ForServerSpawned = new HashSet<TechType>()
        {
            TechType.Shocker,
            TechType.Biter,
            TechType.Blighter,
            TechType.BoneShark,
            TechType.Crabsnake,
            TechType.CrabSquid,
            TechType.Crash,
            TechType.GhostLeviathan,
            TechType.GhostLeviathanJuvenile,
            TechType.GhostRayBlue,
            TechType.GhostRayRed,
            TechType.Mesmer,
            TechType.LavaLizard,
            TechType.LavaEyeye,
            TechType.LavaBoomerang,
            TechType.LargeFloater,
            TechType.LargeKoosh,
            TechType.SpineEel,
            TechType.Spinefish,
            TechType.Sandshark,
            TechType.SeaDragon,
            TechType.SeaEmperor,
            TechType.SeaEmperorBaby,
            TechType.SeaEmperorJuvenile,
            TechType.SeaEmperorLeviathan,
            TechType.ReaperLeviathan,
            TechType.Stalker,
            TechType.Warper,
            TechType.Bladderfish,
            TechType.Boomerang,
            TechType.Cutefish,
            TechType.Eyeye,
            TechType.Jellyray,
            TechType.GarryFish,
            TechType.Gasopod,
            TechType.HoleFish,
            TechType.Hoopfish,
            TechType.Hoverfish,
            TechType.Oculus,
            TechType.RabbitRay,
            TechType.Reefback,
            TechType.Reginald,
            TechType.SeaTreader,
            TechType.Skyray,
            TechType.Spadefish,
            TechType.Spinefish,
            TechType.BlueAmoeba,
            TechType.Shuttlebug,
            TechType.CaveCrawler,
            TechType.Floater,
            TechType.LavaLarva,
            TechType.Rockgrub,
            TechType.Shuttlebug,
            TechType.Bloom,
            TechType.RockPuncher,
            TechType.Peeper
        };
    }
}
