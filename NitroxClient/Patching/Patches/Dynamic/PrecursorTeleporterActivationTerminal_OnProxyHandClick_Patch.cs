using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class PrecursorTeleporterActivationTerminal_OnProxyHandClick_Patch : NitroxPatch, IDynamicPatch
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
}
