using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class KeypadDoorConsole_AcceptNumberField_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((KeypadDoorConsole t) => t.AcceptNumberField());

    public static void Postfix(KeypadDoorConsole __instance)
    {
        KeypadMetadata keypadMetadata = Resolve<KeypadMetadataExtractor>().Extract(__instance);

        NitroxId entityId;
        if (keypadMetadata.PathFromRoot.Length > 0)
        {
            if (!__instance.root.TryGetIdOrWarn(out entityId))
            {
                return;
            }
        }
        else if (!__instance.TryGetIdOrWarn(out entityId))
        {
            return;
        }

        Resolve<Entities>().BroadcastMetadataUpdate(entityId, keypadMetadata);
    }
}
