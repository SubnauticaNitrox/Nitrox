using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxServer_Subnautica.GameLogic.Entities
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
        public static readonly HashSet<NitroxTechType> ForServerSpawned = new HashSet<NitroxTechType>
        {
            TechType.Shocker.ToDto(),
            TechType.Biter.ToDto(),
            TechType.Blighter.ToDto(),
            TechType.BoneShark.ToDto(),
            TechType.Crabsnake.ToDto(),
            TechType.CrabSquid.ToDto(),
            TechType.Crash.ToDto(),
            TechType.GhostLeviathan.ToDto(),
            TechType.GhostLeviathanJuvenile.ToDto(),
            TechType.GhostRayBlue.ToDto(),
            TechType.GhostRayRed.ToDto(),
            TechType.Mesmer.ToDto(),
            TechType.LavaLizard.ToDto(),
            TechType.LavaEyeye.ToDto(),
            TechType.LavaBoomerang.ToDto(),
            TechType.LargeFloater.ToDto(),
            TechType.LargeKoosh.ToDto(),
            TechType.SpineEel.ToDto(),
            TechType.Spinefish.ToDto(),
            TechType.Sandshark.ToDto(),
            TechType.SeaDragon.ToDto(),
            TechType.SeaEmperor.ToDto(),
            TechType.SeaEmperorBaby.ToDto(),
            TechType.SeaEmperorJuvenile.ToDto(),
            TechType.SeaEmperorLeviathan.ToDto(),
            TechType.ReaperLeviathan.ToDto(),
            TechType.Stalker.ToDto(),
            TechType.Warper.ToDto(),
            TechType.Bladderfish.ToDto(),
            TechType.Boomerang.ToDto(),
            TechType.Cutefish.ToDto(),
            TechType.Eyeye.ToDto(),
            TechType.Jellyray.ToDto(),
            TechType.GarryFish.ToDto(),
            TechType.Gasopod.ToDto(),
            TechType.HoleFish.ToDto(),
            TechType.Hoopfish.ToDto(),
            TechType.Hoverfish.ToDto(),
            TechType.Oculus.ToDto(),
            TechType.RabbitRay.ToDto(),
            TechType.Reefback.ToDto(),
            TechType.Reginald.ToDto(),
            TechType.SeaTreader.ToDto(),
            TechType.Skyray.ToDto(),
            TechType.Spadefish.ToDto(),
            TechType.Spinefish.ToDto(),
            TechType.BlueAmoeba.ToDto(),
            TechType.Shuttlebug.ToDto(),
            TechType.CaveCrawler.ToDto(),
            TechType.Floater.ToDto(),
            TechType.LavaLarva.ToDto(),
            TechType.Rockgrub.ToDto(),
            TechType.Shuttlebug.ToDto(),
            TechType.Bloom.ToDto(),
            TechType.RockPuncher.ToDto(),
            TechType.Peeper.ToDto(),
            TechType.Jumper.ToDto()
        };
    }
}
