#if DEBUG
using System.ComponentModel;
using System.Linq;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Factories;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
[Description("Spawns blocks at spawn positions")]
internal sealed class DebugStartMapCommand(RandomFactory randomFactory, RandomStartResource randomStart) : ICommandHandler<int, int>
{
    private readonly RandomFactory randomFactory = randomFactory;
    private readonly RandomStartResource randomStart = randomStart;

    public async Task Execute(ICommandContext context, int amount = 1000, int seed = 0)
    {
        RandomStartGenerator randomResource = await randomStart.GetRandomStartGeneratorAsync();
        NitroxVector3[] randomStartPositions = randomResource.GenerateAllStartPositions(randomFactory.GetDotnetRandom(seed)).Take(amount).ToArray();
        await context.SendToAllAsync(new DebugStartMapPacket(randomStartPositions));
        await context.ReplyAsync($"Rendered {randomStartPositions.Length} spawn positions");
    }
}
#endif
