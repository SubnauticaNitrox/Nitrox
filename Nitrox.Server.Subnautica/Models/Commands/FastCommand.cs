using System.ComponentModel;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
public class FastCommand(SessionSettings sessionSettings) : ICommandHandler<FastCheatChanged.FastCheat>, ICommandHandler<FastCheatChanged.FastCheat, bool>
{
    private readonly SessionSettings sessionSettings = sessionSettings;

    [Description("Enables/disables a fast cheat command, whether it be \"hatch\" or \"grow\"")]
    public async Task Execute(ICommandContext context,
                              [Description("The name of the fast cheat to change: \"hatch\" or \"grow\"")]
                              FastCheatChanged.FastCheat cheat,
                              [Description("Whether the cheat will be enabled or disabled. Default count as a toggle")]
                              bool toggle)
    {
        bool currentCheatValue = IsCheatEnabled(cheat);
        if (currentCheatValue == toggle)
        {
            await context.ReplyAsync($"Fast {cheat} already set to {currentCheatValue}");
            return;
        }

        switch (cheat)
        {
            case FastCheatChanged.FastCheat.HATCH:
                sessionSettings.FastHatch = toggle;
                break;
            case FastCheatChanged.FastCheat.GROW:
                sessionSettings.FastGrow = toggle;
                break;
        }

        await context.SendToAll(new FastCheatChanged(cheat, currentCheatValue));
        await context.SendToAll(new ChatMessage(SessionId.SERVER_ID, $"Fast {cheat} changed to {currentCheatValue} by {context.OriginName}"));
    }

    public async Task Execute(ICommandContext context, FastCheatChanged.FastCheat cheat) => await Execute(context, cheat, !IsCheatEnabled(cheat));

    private bool IsCheatEnabled(FastCheatChanged.FastCheat cheat) =>
        cheat switch
        {
            FastCheatChanged.FastCheat.HATCH => sessionSettings.FastHatch,
            FastCheatChanged.FastCheat.GROW => sessionSettings.FastGrow,
            _ => false
        };
}
