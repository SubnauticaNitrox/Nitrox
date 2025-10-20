using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

/// <summary>
///     Manager for <see cref="PdaStateData" />.
/// </summary>
/// <param name="logger"></param>
internal sealed class PdaManager(ILogger<PdaManager> logger) : ISummarize
{
    private readonly ILogger<PdaManager> logger = logger;
    public PdaStateData PdaState { get; set; } = new();

    public void AddKnownTechType(NitroxTechType techType, List<NitroxTechType> partialTechTypesToRemove)
    {
        PdaState.ScannerPartial.RemoveAll(entry => partialTechTypesToRemove.Contains(entry.TechType));
        if (!PdaState.KnownTechTypes.Contains(techType))
        {
            PdaState.KnownTechTypes.Add(techType);
        }
        else
        {
            logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the KnownTechTypes: [{techType.Name}]");
        }
    }

    public void AddAnalyzedTechType(NitroxTechType techType)
    {
        if (!PdaState.AnalyzedTechTypes.Contains(techType))
        {
            PdaState.AnalyzedTechTypes.Add(techType);
        }
        else
        {
            logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the AnalyzedTechTypes: [{techType.Name}]");
        }
    }

    public void AddEncyclopediaEntry(string entry)
    {
        if (!PdaState.EncyclopediaEntries.Contains(entry))
        {
            PdaState.EncyclopediaEntries.Add(entry);
        }
        else
        {
            logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the EncyclopediaEntries: [{entry}]");
        }
    }

    public void AddPDALogEntry(PDALogEntry entry)
    {
        if (PdaState.PdaLog.All(logEntry => logEntry.Key != entry.Key))
        {
            PdaState.PdaLog.Add(entry);
        }
        else
        {
            logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the PDALog: [{entry.Key}]");
        }
    }

    public void AddScannerFragment(NitroxId id)
    {
        PdaState.ScannerFragments.Add(id);
    }

    public void UpdateEntryUnlockedProgress(NitroxTechType techType, int unlockedAmount, bool fullyResearched)
    {
        if (fullyResearched)
        {
            PdaState.ScannerPartial.RemoveAll(entry => entry.TechType.Equals(techType));
            PdaState.ScannerComplete.Add(techType);
        }
        else
        {
            lock (PdaState.ScannerPartial)
            {
                if (PdaState.ScannerPartial.FirstOrDefault(e => e.TechType.Equals(techType)) is { } entry)
                {
                    entry.Unlocked = unlockedAmount;
                }
                else
                {
                    PdaState.ScannerPartial.Add(new(techType, unlockedAmount));
                }
            }
        }
    }

    public InitialPDAData GetInitialPDAData()
    {
        return new(PdaState.KnownTechTypes.ToList(),
                   PdaState.AnalyzedTechTypes.ToList(),
                   PdaState.PdaLog.ToList(),
                   PdaState.EncyclopediaEntries.ToList(),
                   PdaState.ScannerFragments.ToList(),
                   PdaState.ScannerPartial.ToList(),
                   PdaState.ScannerComplete.ToList());
    }

    public void RemovePdaLogsByKey(string eventKey) => PdaState.PdaLog.RemoveAll(entry => entry.Key == eventKey);

    public bool ContainsCompletelyScannedTech(NitroxTechType techType) => PdaState.ScannerComplete.Contains(techType);

    public bool ContainsLog(string pdaEntryId) => PdaState.PdaLog.Any(entry => entry.Key == pdaEntryId);

    public Task LogSummaryAsync(Perms viewerPerms)
    {
        logger.ZLogInformation($"Known tech: {PdaState.KnownTechTypes.Count}");
        logger.ZLogInformation($"Encyclopedia entries: {PdaState.EncyclopediaEntries.Count}");
        return Task.CompletedTask;
    }
}
