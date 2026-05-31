using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class LeakRepairedProcessor(WorldEntityManager worldEntityManager, SaveService saveService, ILogger<LeakRepairedProcessor> logger) : IAuthPacketProcessor<LeakRepaired>
{
    private static readonly Lock saveDebounceLock = new();
    private static DateTime lastSaveRequest = DateTime.MinValue;
    private static readonly TimeSpan saveDebounceInterval = TimeSpan.FromSeconds(5);

    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SaveService saveService = saveService;
    private readonly ILogger<LeakRepairedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, LeakRepaired packet)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            await context.SendToOthersAsync(packet);
            
            // Trigger a debounced save to persist the leak removal
            // This ensures that if the server crashes or a player joins before the next autosave,
            // the repaired leak won't reappear
            await TriggerDebouncedSaveAsync();
        }
    }

    private async Task TriggerDebouncedSaveAsync()
    {
        bool shouldTriggerSave = false;
        
        lock (saveDebounceLock)
        {
            DateTime now = DateTime.UtcNow;
            if (now - lastSaveRequest >= saveDebounceInterval)
            {
                lastSaveRequest = now;
                shouldTriggerSave = true;
            }
        }

        if (shouldTriggerSave)
        {
            logger.ZLogDebug($"Queueing save after base leak repair to ensure persistence");
            await saveService.QueueActionAsync(SaveService.ServiceAction.SAVE);
        }
    }
}
