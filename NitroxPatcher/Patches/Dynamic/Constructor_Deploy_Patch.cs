using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Constructor_Deploy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructor t) => t.Deploy(default(bool)));

    public static void Prefix(Constructor __instance, bool value)
    {
        // only trigger updates when there is a valid state change.
        if (value != __instance.deployed)
        {
            // We need to set this early so that the extracted metadata has the right value for "deployed"
            __instance.deployed = value;
            if (__instance.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Entities>().EntityMetadataChanged(__instance, id);
            }
        }
    }
}
