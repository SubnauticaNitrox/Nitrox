#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal class DebugStartMapCommand(IOptions<Configuration.SubnauticaServerOptions> optionsProvider, RandomStartResource randomStart) : ICommandHandler
{
    private readonly RandomStartResource randomStart = randomStart;
    private readonly IOptions<Configuration.SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Spawns blocks at spawn positions")]
    public async Task Execute(ICommandContext context)
    {
        RandomStartGenerator rng = await randomStart.LoadAndGetRandomStartGeneratorAsync();
        List<NitroxVector3> randomStartPositions = rng.GenerateRandomStartPositions(optionsProvider.Value.Seed);

        await context.SendToAll(new DebugStartMapPacket(randomStartPositions));
        await context.ReplyAsync($"Rendered {randomStartPositions.Count} spawn positions");
    }
}
#endif
