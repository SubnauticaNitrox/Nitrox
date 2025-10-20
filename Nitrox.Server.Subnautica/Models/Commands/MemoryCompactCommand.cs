using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class MemoryCompactCommand : Command
{
    private readonly MemoryService memoryService;

    public MemoryCompactCommand(MemoryService memoryService) : base("memorycompact", Perms.HOST, "Compacts memory")
    {
        this.memoryService = memoryService;
    }

    protected override void Execute(CallArgs args)
    {
        memoryService.QueueCompact();
    }
}
