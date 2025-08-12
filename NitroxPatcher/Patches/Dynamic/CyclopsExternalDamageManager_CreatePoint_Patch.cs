using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsExternalDamageManager t) => t.CreatePoint());

    public static bool Prefix(CyclopsExternalDamageManager __instance, out bool __state)
    {
        // Block from creating points if they aren't the owner of the sub
        __state = __instance.subRoot.TryGetNitroxId(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id);

        return __state;
    }

    public static void Postfix(CyclopsExternalDamageManager __instance, bool __state)
    {
        if (__state)
        {
            Resolve<Cyclops>().OnCreateDamagePoint(__instance.subRoot);
        }
    }
}
