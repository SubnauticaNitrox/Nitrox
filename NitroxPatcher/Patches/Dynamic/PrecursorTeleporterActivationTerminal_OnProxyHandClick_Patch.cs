using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class PrecursorTeleporterActivationTerminal_OnProxyHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorTeleporterActivationTerminal t) => t.OnProxyHandClick(default(GUIHand)));

    public static void Postfix(PrecursorTeleporterActivationTerminal __instance)
    {
        if (__instance.unlocked && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            PrecursorTeleporterActivationTerminalMetadata precursorTeleporterActivationTerminalMetadata = new(__instance.unlocked);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorTeleporterActivationTerminalMetadata);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
