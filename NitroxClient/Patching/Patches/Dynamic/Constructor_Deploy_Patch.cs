using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

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
