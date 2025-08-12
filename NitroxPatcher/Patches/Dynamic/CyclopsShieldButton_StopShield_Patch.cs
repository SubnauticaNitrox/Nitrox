using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsShieldButton_StopShield_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsShieldButton t) => t.StopShield());

    public static void Postfix(CyclopsShieldButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Cyclops>().BroadcastMetadataChange(id);
        }
    }
}
