using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

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
