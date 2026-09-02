using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Model.Subnautica.Helper;

public static class SimulationWhitelist
{
    /// <summary>
    ///     We don't want to give out simulation to all entities that the server sent out because there is a lot of stationary items and junk (TechType.None).
    ///     It is easier to maintain a list of items we should simulate than try to blacklist items. This list should not be checked for non-server spawned items
    ///     as they were probably dropped by the player and are mostly guaranteed to move.
    /// </summary>
    private static readonly HashSet<NitroxTechType> movementWhitelist =
    [
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
        TechType.Jumper.ToDto(),
        TechType.Constructor.ToDto(),
        TechType.PipeSurfaceFloater.ToDto()
    ];

    /// <summary>
    ///     We differentiate the entities which should be simulated because of one of their behaviour (ie for utility)
    ///     from those are simulated for their movements.
    /// </summary>
    private static readonly HashSet<NitroxTechType> utilityWhitelist = new()
    {
        TechType.CrashHome.ToDto()
    };

    public static bool ShouldSimulateEntity(WorldEntity entity)
    {
        return utilityWhitelist.Contains(entity.TechType) || ShouldSimulateEntityMovement(entity);
    }

    public static bool ShouldSimulateEntityMovement(WorldEntity entity)
    {
        return !entity.SpawnedByServer || movementWhitelist.Contains(entity.TechType);
    }
}
