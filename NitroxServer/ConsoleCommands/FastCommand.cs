using System;
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
        AddParameter(new TypeEnum<FastCheatChanged.FastCheat>("cheat", true, "The name of the fast cheat to change: \"hatch\" or \"grow\""));
        AddParameter(new TypeBoolean("value", false, "Whether the cheat will be enabled or disabled. Default count as a toggle"));
        this.playerManager = playerManager;
        this.sessionSettings = sessionSettings;
    }

    protected override void Execute(CallArgs args)
    {
        FastCheatChanged.FastCheat cheat = args.Get<FastCheatChanged.FastCheat>(0);

        bool value = cheat switch
        {
            FastCheatChanged.FastCheat.HATCH => sessionSettings.FastHatch,
            FastCheatChanged.FastCheat.GROW => sessionSettings.FastGrow,
            _ => throw new ArgumentException("Must provide a valid cheat name: \"hatch\" or \"grow\""),
        };


        if (args.IsValid(1))
        {
            bool newValue = args.Get<bool>(1);
            if (newValue == value)
            {
                SendMessage(args.Sender, $"Fast {cheat} already set to {newValue}");
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
            case FastCheatChanged.FastCheat.HATCH:
                sessionSettings.FastHatch = value;
                break;
            case FastCheatChanged.FastCheat.GROW:
                sessionSettings.FastGrow = value;
                break;
        }

        playerManager.SendPacketToAllPlayers(new FastCheatChanged(cheat, value));
        SendMessageToAllPlayers($"Fast {cheat} changed to {value} by {args.SenderName}");
    }
}
