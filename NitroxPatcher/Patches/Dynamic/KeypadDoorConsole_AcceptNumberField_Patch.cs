using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class KeypadDoorConsole_AcceptNumberField_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((KeypadDoorConsole t) => t.AcceptNumberField());

    public static void Postfix(KeypadDoorConsole __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            KeypadMetadata keypadMetadata = new(__instance.unlocked);
            Resolve<Entities>().BroadcastMetadataUpdate(id, keypadMetadata);
        }
    }
}
