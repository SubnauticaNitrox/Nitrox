#if DEBUG
using System;
using System.Collections.Generic;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.ConsoleCommands;

internal class PlayerCommand : Command
{
    private readonly Lazy<PlayerManager> playerManager = new(NitroxServiceLocator.LocateService<PlayerManager>);
    private readonly Lazy<WorldEntityManager> worldEntityManager = new(NitroxServiceLocator.LocateService<WorldEntityManager>);
    private readonly Lazy<SimulationOwnershipData> simulationOwnershipData = new(NitroxServiceLocator.LocateService<SimulationOwnershipData>);

    public PlayerCommand() : base("player", Perms.CONSOLE, "Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")
    {
        AddParameter(new TypeString("player name", true, "name of the target player"));
    }

    protected override void Execute(CallArgs args)
    {
        string playerName = args.Get<string>(0);

        if (playerManager.Value.TryGetPlayerByName(playerName, out Player player))
        {
            List<AbsoluteEntityCell> visibleCells = player.GetVisibleCells();

            Log.Info(player);
            Log.Info($"Visible cells [{visibleCells.Count}]:");
            foreach (AbsoluteEntityCell visibleCell in visibleCells)
            {
                string simulatedEntities = "";
                foreach (WorldEntity worldEntity in worldEntityManager.Value.GetEntities(visibleCell))
                {
                    if (simulationOwnershipData.Value.TryGetLock(worldEntity.Id, out SimulationOwnershipData.PlayerLock playerLock) &&
                        playerLock.Player.Id == player.Id)
                    {
                        simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
                    }
                }
                Log.Info($"{visibleCell}; {NitroxVector3.Distance(visibleCell.Position, player.Position)}");
                if (simulatedEntities.Length > 0)
                {
                    // Get everything but the last ", " of the string
                    Log.Info(simulatedEntities[..^2]);
                }
            }
            Log.Info($"\nOut of cell entities:\n{string.Join(", ", player.OutOfCellVisibleEntities)}");
        }
        else
        {
            Log.Error($"Player with name {playerName} not found");
        }
    }
}
#endif
