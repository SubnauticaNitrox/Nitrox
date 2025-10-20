using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class GameModeCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly ILogger<GameModeCommand> logger;

    public GameModeCommand(PlayerManager playerManager, ILogger<GameModeCommand> logger) : base("gamemode", Perms.ADMIN, "Changes a player's gamemode")
    {
        AddParameter(new TypeEnum<SubnauticaGameMode>("gamemode", true, "Gamemode to change to"));
        AddParameter(new TypePlayer("name", false, "Username to whom change the game mode (defaults to self)"));

        this.playerManager = playerManager;
        this.logger = logger;
    }

    protected override void Execute(CallArgs args)
    {
        SubnauticaGameMode gameMode = args.Get<SubnauticaGameMode>(0);
        Player targetPlayer = args.Get<Player>(1);

        if (args.IsConsole && targetPlayer == null)
        {
            logger.ZLogError($"Console can't use the gamemode command without providing a player name to it.");
            return;
        }
        // The target player if not set, is the player who sent the command
        targetPlayer ??= args.Sender.Value;

        targetPlayer.GameMode = gameMode;
        playerManager.SendPacketToAllPlayers(GameModeChanged.ForPlayer(targetPlayer.Id, gameMode));
        SendMessage(targetPlayer, $"GameMode changed to {gameMode}");
        if (args.IsConsole)
        {
            logger.ZLogInformation($"Changed {targetPlayer.Name} [{targetPlayer.Id}]'s gamemode to {gameMode}");
        }
        else
        {
            if (targetPlayer != args.Sender.Value)
            {
                SendMessage(args.Sender.Value, $"GameMode of {targetPlayer.Name} changed to {gameMode}");
            }
        }
    }
}
