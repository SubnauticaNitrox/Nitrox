#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal sealed class DebugStartMapCommand(IOptions<SubnauticaServerOptions> optionsProvider, RandomStartResource randomStart) : ICommandHandler
{
    private readonly RandomStartResource randomStart = randomStart;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Spawns blocks at spawn positions")]
    public async Task Execute(ICommandContext context)
    {
        List<NitroxVector3> randomStartPositions = randomStart.RandomStartGenerator.GenerateRandomStartPositions(optionsProvider.Value.Seed);

        await context.SendToAllAsync(new DebugStartMapPacket(randomStartPositions));
        await context.ReplyAsync($"Rendered {randomStartPositions.Count} spawn positions");
    }
}
#endif
