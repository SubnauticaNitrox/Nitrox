using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

/// <summary>
///     Manager for thread-safe access to <see cref="PdaStateData" />.
/// </summary>
internal sealed class PdaManager(ILogger<PdaManager> logger) : ISummarize
{
    private readonly ILogger<PdaManager> logger = logger;

    private readonly Lock pdaStateLock = new();

    /// <summary>
    ///     Any access to this state must use the same thread-safe lock to avoid one list being updated based on the invalid
    ///     state of another.
    /// </summary>
    public PdaStateData PdaState
    {
        private get;
        set
        {
            lock (pdaStateLock)
            {
                field = value;
            }
        }
    } = new();

    public PdaStateData GetPdaStateCopy()
    {
        lock (pdaStateLock)
        {
            return PdaState.GetFullCopy();
        }
    }

    public void AddKnownTechType(NitroxTechType techType, List<NitroxTechType> partialTechTypesToRemove)
    {
        bool duplicateAttempt = false;
        try
        {
            lock (pdaStateLock)
            {
                PdaState.ScannerPartial.RemoveAllFast(partialTechTypesToRemove, static (entry, list) => list.Contains(entry.TechType));
                if (PdaState.KnownTechTypes.Contains(techType))
                {
                    duplicateAttempt = true;
                    return;
                }
                PdaState.KnownTechTypes.Add(techType);
            }
        }
        finally
        {
            if (duplicateAttempt)
            {
                logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the {nameof(PdaState.KnownTechTypes)}: [{techType.Name}]");
            }
        }
    }

    public void AddAnalyzedTechType(NitroxTechType techType)
    {
        bool duplicateAttempt = false;
        try
        {
            lock (pdaStateLock)
            {
                if (PdaState.AnalyzedTechTypes.Contains(techType))
                {
                    duplicateAttempt = true;
                    return;
                }
                PdaState.AnalyzedTechTypes.Add(techType);
            }
        }
        finally
        {
            if (duplicateAttempt)
            {
                logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the {nameof(PdaState.AnalyzedTechTypes)}: [{techType.Name}]");
            }
        }
    }

    public void AddEncyclopediaEntry(string entry)
    {
        bool duplicateAttempt = false;
        try
        {
            lock (pdaStateLock)
            {
                if (PdaState.EncyclopediaEntries.Contains(entry))
                {
                    duplicateAttempt = true;
                    return;
                }
                PdaState.EncyclopediaEntries.Add(entry);
            }
        }
        finally
        {
            if (duplicateAttempt)
            {
                logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the {nameof(PdaState.EncyclopediaEntries)}: [{entry}]");
            }
        }
    }

    public void AddPDALogEntry(PDALogEntry entry)
    {
        bool duplicateAttempt = false;
        try
        {
            lock (pdaStateLock)
            {
                if (PdaState.PdaLog.Any(logEntry => logEntry.Key == entry.Key))
                {
                    duplicateAttempt = true;
                    return;
                }
                PdaState.PdaLog.Add(entry);
            }
        }
        finally
        {
            if (duplicateAttempt)
            {
                logger.ZLogDebug($"There was an attempt of adding a duplicated entry in the {nameof(PdaState.PdaLog)}: [{entry.Key}]");
            }
        }
    }

    public void AddScannerFragment(NitroxId id)
    {
        lock (pdaStateLock)
        {
            PdaState.ScannerFragments.Add(id);
        }
    }

    public void UpdateEntryUnlockedProgress(NitroxTechType techType, int unlockedAmount, bool fullyResearched)
    {
        lock (pdaStateLock)
        {
            if (fullyResearched)
            {
                PdaState.ScannerPartial.RemoveAllFast(techType, static (entry, toRemove) => entry.TechType.Equals(toRemove));
                PdaState.ScannerComplete.Add(techType);
            }
            else
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
        lock (pdaStateLock)
        {
            return new(PdaState.KnownTechTypes.ToList(),
                       PdaState.AnalyzedTechTypes.ToList(),
                       PdaState.PdaLog.ToList(),
                       PdaState.EncyclopediaEntries.ToList(),
                       PdaState.ScannerFragments.ToList(),
                       PdaState.ScannerPartial.ToList(),
                       PdaState.ScannerComplete.ToList());
        }
    }

    public void RemovePdaLogsByKey(string eventKey)
    {
        lock (pdaStateLock)
        {
            PdaState.PdaLog.RemoveAllFast(eventKey, static (entry, keyToRemove) => entry.Key == keyToRemove);
        }
    }

    public bool ContainsCompletelyScannedTech(NitroxTechType techType)
    {
        lock (pdaStateLock)
        {
            return PdaState.ScannerComplete.Contains(techType);
        }
    }

    public bool ContainsLog(string pdaEntryId)
    {
        lock (pdaStateLock)
        {
            return PdaState.PdaLog.Any(entry => entry.Key == pdaEntryId);
        }
    }

    Task IEvent<ISummarize.Args>.OnEventAsync(ISummarize.Args args)
    {
        int knownTechCount;
        int encyclopediaEntriesCount;
        lock (pdaStateLock)
        {
            knownTechCount = PdaState.KnownTechTypes.Count;
            encyclopediaEntriesCount = PdaState.EncyclopediaEntries.Count;
        }
        logger.ZLogInformation($"Known tech: {knownTechCount}");
        logger.ZLogInformation($"Encyclopedia entries: {encyclopediaEntriesCount}");

        return Task.CompletedTask;
    }
}
