using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FireExtinguisher_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((FireExtinguisher t) => t.Update());

    public static bool Prefix(FireExtinguisher __instance)
    {
        if (__instance.transform.TryGetComponentInAscendance(12, out RemotePlayerIdentifier identifier))
        {
            Resolve<FireExtinguishers>().RemoteUpdate(__instance, identifier);
            return false;
        }

        return true;
    }

    public static void Postfix(FireExtinguisher __instance, bool __runOriginal)
    {
        if (__runOriginal && __instance.TryGetNitroxId(out NitroxId id))
        {
            Resolve<FireExtinguishers>().BroadcastLocalState(id, __instance);
        }
    }
}
