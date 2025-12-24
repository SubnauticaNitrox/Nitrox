#if DEBUG
using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class PlayerCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly WorldEntityManager worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly ILogger<PlayerCommand> logger;

    public PlayerCommand(PlayerManager playerManager, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, ILogger<PlayerCommand> logger) : base("player", Perms.HOST, "Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
        this.simulationOwnershipData = simulationOwnershipData;
        this.logger = logger;
        AddParameter(new TypeString("player name", true, "name of the target player"));
    }

    protected override void Execute(CallArgs args)
    {
        string playerName = args.Get<string>(0);

        if (playerManager.TryGetPlayerByName(playerName, out Player player))
        {
            List<AbsoluteEntityCell> visibleCells = player.GetVisibleCells();

            logger.ZLogInformation($"{player}");
            logger.ZLogInformation($"Visible cells [{visibleCells.Count}]:");
            foreach (AbsoluteEntityCell visibleCell in visibleCells)
            {
                string simulatedEntities = "";
                foreach (WorldEntity worldEntity in worldEntityManager.GetEntities(visibleCell))
                {
                    if (simulationOwnershipData.TryGetLock(worldEntity.Id, out SimulationOwnershipData.PlayerLock playerLock) &&
                        playerLock.Player.Id == player.Id)
                    {
                        simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
                    }
                }
                logger.ZLogInformation($"{visibleCell}; {NitroxVector3.Distance(visibleCell.Position, player.Position)}");
                if (simulatedEntities.Length > 0)
                {
                    // Get everything but the last ", " of the string
                    logger.ZLogInformation($"{simulatedEntities[..^2]}");
                }
            }
            logger.ZLogInformation($"\nOut of cell entities:\n{string.Join(", ", player.OutOfCellVisibleEntities)}");
        }
        else
        {
            logger.ZLogError($"Player with name {playerName} not found");
        }
    }
}
#endif
