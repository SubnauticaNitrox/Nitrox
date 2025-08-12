using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/*
 * Relays Cyclops FireSuppressionSystem to other players
 * This method was used instead of the OnClick to ensure, that the the suppression really started
 */
public sealed partial class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsFireSuppressionSystemButton t) => t.StartCooldown());

    public static void Postfix(CyclopsFireSuppressionSystemButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateFireSuppression(id);
        }
    }
}
