using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class CyclopsDecoyLaunchButton_OnClick_Patch : NitroxPatch, IDynamicPatch
{
    private static Cyclops cyclops;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDecoyLaunchButton t) => t.OnClick());

    public CyclopsDecoyLaunchButton_OnClick_Patch(Cyclops c)
    {
        cyclops = c;
    }

    public static void Postfix(CyclopsHornButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            cyclops.BroadcastLaunchDecoy(id);
        }
    }
}
