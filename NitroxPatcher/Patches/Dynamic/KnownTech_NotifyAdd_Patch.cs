using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public class KnownTech_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => KnownTech.NotifyAdd(default, default));

    public static void Prefix(TechType techType, bool verbose)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        List<PDAScanner.Entry> partialEntries = new();
        PDAScanner.GetPartialEntriesWhichUnlocks(techType, partialEntries, true);

        Resolve<IPacketSender>().Send(new KnownTechEntryAdd(
            KnownTechEntryAdd.EntryCategory.KNOWN,
            techType.ToDto(),
            verbose,
            partialEntries.Select(entry => entry.techType.ToDto()).ToList()
        ));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
