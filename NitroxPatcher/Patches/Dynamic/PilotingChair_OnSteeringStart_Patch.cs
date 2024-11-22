using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PilotingChair_OnSteeringStart_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PilotingChair t) => t.OnSteeringStart(default));

    public static void Postfix(PilotingChair __instance)
    {
        if (Player.main.currChair == __instance && __instance.subRoot)
        {
            Resolve<Vehicles>().BroadcastOnPilotModeChanged(__instance.subRoot.gameObject, true);
        }
    }
}
