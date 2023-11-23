using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class RocketPreflightCheckManager_CompletePreflightCheck_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((RocketPreflightCheckManager t) => t.CompletePreflightCheck(default(PreflightCheck)));

    public static void Postfix(RocketPreflightCheckManager __instance)
    {
        Rocket rocket = __instance.gameObject.RequireComponentInParent<Rocket>();

        if (rocket.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(rocket, id);
        }
    }
}
