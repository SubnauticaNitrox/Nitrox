#if DEBUG
using System.ComponentModel;
using System.Linq;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Factories;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[Alias("seednearpos")]
[RequiresPermission(Perms.ADMIN)] // This command is CPU heavy: admin perms.
[Description("Gets a world seed where the spawn position is the closest to the X and Z coordinates")]
internal sealed class SeedNearPositionCommand(RandomStartResource randomStart) : ICommandHandler<int, int>, ICommandHandler<int, int, int>
{
    private readonly RandomStartResource randomStart = randomStart;

    public async Task Execute(ICommandContext context, int x, int z, int iterations)
    {
        iterations = int.Max(1000, iterations);
        RandomStartGenerator randomResource = await randomStart.GetRandomStartGeneratorAsync();
        NitroxVector3 desiredPos = new(x, 0, z);
        LoopState best = new(0, null, "");
        Lock bestLock = new();
        Parallel.For<LoopState>(0,
                                iterations,
                                () => new(0, null, RandomFactory.GetCsFilePathFromType(typeof(EscapePodManager))),
                                (i, _, state) =>
                                {
                                    NitroxVector3 currentPos = randomResource.GenerateAllStartPositions(new Random(RandomFactory.CreateSeedInt32(i.ToString(), state.CsFilePathForSeed))).FirstOrDefault();
                                    if (state.Position == null || NitroxVector3.Distance(currentPos, desiredPos) < NitroxVector3.Distance(state.Position.Value, desiredPos))
                                    {
                                        state.Position = currentPos;
                                        state.Seed = i;
                                    }
                                    return state;
                                }, state =>
                                {
                                    if (!state.Position.HasValue)
                                    {
                                        return;
                                    }
                                    lock (bestLock)
                                    {
                                        if (!best.Position.HasValue || NitroxVector3.Distance(state.Position.Value, desiredPos) < NitroxVector3.Distance(best.Position.Value, desiredPos))
                                        {
                                            best.Position = state.Position;
                                            best.Seed = state.Seed;
                                        }
                                    }
                                });
        if (!best.Position.HasValue)
        {
            await context.ReplyAsync($"Failed to generate a seed close to {desiredPos}");
            return;
        }

        await context.ReplyAsync($"Seed \"{best.Seed}\" is near to {desiredPos}. Actual position {best.Position} which is {NitroxVector3.Distance(best.Position.Value, desiredPos)} units off target.");
    }

    [RequiresPermission(Perms.PLAYER)] // Lower iteration count makes it OK to call with weak perms.
    public async Task Execute(ICommandContext context, int x, int z) => await Execute(context, x, z, 1000);

    private record LoopState(int Seed, NitroxVector3? Position, string CsFilePathForSeed)
    {
        public NitroxVector3? Position = Position;
        public int Seed = Seed;
    }
}
#endif
