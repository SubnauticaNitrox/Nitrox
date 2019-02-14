using System.Collections.Generic;
using NitroxModel_Subnautica.Helper;
using TechTypeModel = NitroxModel.DataStructures.TechType;

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
        public static readonly HashSet<TechTypeModel> ForServerSpawned = new HashSet<TechTypeModel>()
        {
            TechType.Shocker.Model(),
            TechType.Biter.Model(),
            TechType.Blighter.Model(),
            TechType.BoneShark.Model(),
            TechType.Crabsnake.Model(),
            TechType.CrabSquid.Model(),
            TechType.Crash.Model(),
            TechType.GhostLeviathan.Model(),
            TechType.GhostLeviathanJuvenile.Model(),
            TechType.GhostRayBlue.Model(),
            TechType.GhostRayRed.Model(),
            TechType.Mesmer.Model(),
            TechType.LavaLizard.Model(),
            TechType.LavaEyeye.Model(),
            TechType.LavaBoomerang.Model(),
            TechType.LargeFloater.Model(),
            TechType.LargeKoosh.Model(),
            TechType.SpineEel.Model(),
            TechType.Spinefish.Model(),
            TechType.Sandshark.Model(),
            TechType.SeaDragon.Model(),
            TechType.SeaEmperor.Model(),
            TechType.SeaEmperorBaby.Model(),
            TechType.SeaEmperorJuvenile.Model(),
            TechType.SeaEmperorLeviathan.Model(),
            TechType.ReaperLeviathan.Model(),
            TechType.Stalker.Model(),
            TechType.Warper.Model(),
            TechType.Bladderfish.Model(),
            TechType.Boomerang.Model(),
            TechType.Cutefish.Model(),
            TechType.Eyeye.Model(),
            TechType.Jellyray.Model(),
            TechType.GarryFish.Model(),
            TechType.Gasopod.Model(),
            TechType.HoleFish.Model(),
            TechType.Hoopfish.Model(),
            TechType.Hoverfish.Model(),
            TechType.Oculus.Model(),
            TechType.RabbitRay.Model(),
            TechType.Reefback.Model(),
            TechType.Reginald.Model(),
            TechType.SeaTreader.Model(),
            TechType.Skyray.Model(),
            TechType.Spadefish.Model(),
            TechType.Spinefish.Model(),
            TechType.BlueAmoeba.Model(),
            TechType.Shuttlebug.Model(),
            TechType.CaveCrawler.Model(),
            TechType.Floater.Model(),
            TechType.LavaLarva.Model(),
            TechType.Rockgrub.Model(),
            TechType.Shuttlebug.Model(),
            TechType.Bloom.Model(),
            TechType.RockPuncher.Model(),
            TechType.Peeper.Model()
        };
    }
}
