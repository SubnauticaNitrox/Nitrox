using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class PrecursorKeyTerminal_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorKeyTerminal t) => t.OnHandClick(default(GUIHand)));

    public static void Postfix(PrecursorKeyTerminal __instance)
    {
        if (__instance.slotted && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            PrecursorKeyTerminalMetadata precursorKeyTerminalMetadata = new(__instance.slotted);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorKeyTerminalMetadata);
        }
    }
}
