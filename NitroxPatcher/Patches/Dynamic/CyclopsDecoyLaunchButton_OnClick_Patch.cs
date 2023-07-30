using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsDecoyLaunchButton_OnClick_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDecoyLaunchButton t) => t.OnClick());

    public static void Postfix(CyclopsHornButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastLaunchDecoy(id);
        }
    }
}
