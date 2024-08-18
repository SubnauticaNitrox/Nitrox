using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.InitialSync;

public sealed class PdaInitialSyncProcessor : InitialSyncProcessor
{
    public PdaInitialSyncProcessor()
    {
        AddDependency<ClockSyncInitialSyncProcessor>();
    }

    // The steps are ordered like their call order in Player.OnProtoDeserialize
    public override List<Func<InitialPlayerSync, IEnumerator>> Steps { get; } =
    [
        RestoreKnownTech,
        RestorePDALog,
        RestoreEncyclopediaEntries,
        RestorePDAScanner
    ];

    private static IEnumerator RestoreKnownTech(InitialPlayerSync packet)
    {
        List<TechType> knownTech = packet.PDAData.KnownTechTypes.Select(techType => techType.ToUnity()).ToList();
        HashSet<TechType> analyzedTech = new(packet.PDAData.AnalyzedTechTypes.Select(techType => techType.ToUnity()));
        Log.Info($"Received initial sync packet with {knownTech.Count} KnownTech.knownTech types and {analyzedTech.Count} KnownTech.analyzedTech types.");

        using (PacketSuppressor<KnownTechEntryAdd>.Suppress())
        {
            KnownTech.Deserialize(knownTech, analyzedTech);
        }
        yield break;
    }

    private static IEnumerator RestorePDALog(InitialPlayerSync packet)
    {
        List<PDALogEntry> logEntries = packet.PDAData.PDALogEntries;
        Log.Info($"Received initial sync packet with {logEntries.Count} pda log entries");

        using (PacketSuppressor<PDALogEntryAdd>.Suppress())
        {
            // We just need the timestamp and the key because everything else is provided by PDALog.InitDataForEntries
            PDALog.Deserialize(logEntries.ToDictionary(m => m.Key, m => new PDALog.Entry() { timestamp = m.Timestamp }));
        }
        yield break;
    }

    private static IEnumerator RestoreEncyclopediaEntries(InitialPlayerSync packet)
    {
        List<string> entries = packet.PDAData.EncyclopediaEntries;
        Log.Info($"Received initial sync packet with {entries.Count} encyclopedia entries");

        using (PacketSuppressor<PDAEncyclopediaEntryAdd>.Suppress())
        {
            // We don't do as in PDAEncyclopedia.Deserialize because we don't persist the entry's fields which are useless
            foreach (string entry in entries)
            {
#if SUBNAUTICA
                PDAEncyclopedia.Add(entry, false);
#elif BELOWZERO
                PDAEncyclopedia.Add(entry, false, false);
#endif
            }
        }
        yield break;
    }

    private static IEnumerator RestorePDAScanner(InitialPlayerSync packet)
    {
        InitialPDAData pdaData = packet.PDAData;

        PDAScanner.Data data = new()
        {
            fragments = pdaData.ScannerFragments.ToDictionary(m => m.ToString(), m => 1f),
            partial = pdaData.ScannerPartial.Select(entry => entry.ToUnity()).ToList(),
            complete = new HashSet<TechType>(pdaData.ScannerComplete.Select(techType => techType.ToUnity()))
        };
        PDAScanner.Deserialize(data);
        yield break;
    }
}
