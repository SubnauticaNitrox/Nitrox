using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class KnownTech_NotifyAnalyze_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => KnownTech.NotifyAnalyze(default, default));

    public static void Prefix(KnownTech.AnalysisTech analysis, bool verbose)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        Resolve<IPacketSender>().Send(new KnownTechEntryAdd(KnownTechEntryAdd.EntryCategory.ANALYZED, analysis.techType.ToDto(), verbose));
    }
}
