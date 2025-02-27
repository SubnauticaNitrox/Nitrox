using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

public class FastCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly SessionSettings sessionSettings;

    public FastCommand(PlayerManager playerManager, SessionSettings sessionSettings) : base("fast", Perms.MODERATOR, "Enables/disables a fast cheat command, whether it be \"hatch\" or \"grow\"")
    {
        AddParameter(new TypeString("cheat", true, "The name of the fast cheat to change: \"hatch\" or \"grow\""));
        AddParameter(new TypeBoolean("value", false, "Whether the cheat will be enabled or disabled. Default count as a toggle"));
        this.playerManager = playerManager;
        this.sessionSettings = sessionSettings;
    }

    protected override void Execute(CallArgs args)
    {
        string cheatName = args.Get<string>(0).ToLowerInvariant();

        FastCheatChanged.FastCheat cheat;
        bool value;
        switch (cheatName)
        {
            case "hatch":
                cheat = FastCheatChanged.FastCheat.FAST_HATCH;
                value = sessionSettings.FastHatch;
                break;

            case "grow":
                cheat = FastCheatChanged.FastCheat.FAST_GROW;
                value = sessionSettings.FastGrow;
                break;

            default:
                SendMessage(args.Sender, "Must provide a valid cheat name: \"hatch\" or \"grow\"");
                return;
        }

        if (args.IsValid(1))
        {
            bool newValue = args.Get<bool>(1);
            if (newValue == value)
            {
                SendMessage(args.Sender, $"Fast {cheatName} already set to {newValue}");
                return;
            }
            value = newValue;
        }
        else
        {
            // If the value wasn't provided then we toggle it
            value = !value;
        }

        switch (cheat)
        {
            case FastCheatChanged.FastCheat.FAST_HATCH:
                sessionSettings.FastHatch = value;
                break;
            case FastCheatChanged.FastCheat.FAST_GROW:
                sessionSettings.FastGrow = value;
                break;
        }

        playerManager.SendPacketToAllPlayers(new FastCheatChanged(cheat, value));
        SendMessageToAllPlayers($"Fast {cheatName} changed to {value} by {args.SenderName}");
    }
}
