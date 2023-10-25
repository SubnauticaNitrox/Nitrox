using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

internal class GameModeCommand : Command
{
    private readonly PlayerManager playerManager;

    public GameModeCommand(PlayerManager playerManager) : base("gamemode", Perms.ADMIN, "Changes a player's gamemode")
    {
        AddParameter(new TypeEnum<NitroxGameMode>("gamemode", true, "Gamemode to change to"));
        AddParameter(new TypePlayer("name", false, "Username to whom change the game mode (defaults to self)"));

        this.playerManager = playerManager;
    }

    protected override void Execute(CallArgs args)
    {
        NitroxGameMode gameMode = args.Get<NitroxGameMode>(0);
        Player targetPlayer = args.Get<Player>(1);

        if (args.IsConsole && targetPlayer == null)
        {
            Log.Error($"Console can't use the gamemode command without providing a player name to it.");
            return;
        }
        // The target player if not set, is the player who sent the command
        targetPlayer ??= args.Sender.Value;

        playerManager.SendPacketToAllPlayers(GameModeChanged.ForPlayer(targetPlayer.Id, gameMode));
        SendMessage(targetPlayer, $"GameMode changed to {gameMode}");
        if (args.IsConsole)
        {
            Log.Info($"Changed {targetPlayer.Name} [{targetPlayer.Id}]'s gamemode to {gameMode}");
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
