using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/*
 * Relays Cyclops FireSuppressionSystem to other players
 * This method was used instead of the OnClick to ensure, that the the suppression really started
 */
internal sealed partial class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch, IDynamicPatch
{
    private static Cyclops cyclops;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsFireSuppressionSystemButton t) => t.StartCooldown());

    public CyclopsFireSuppressionButton_StartCooldown_Patch(Cyclops c)
    {
        cyclops = c ?? throw new ArgumentNullException(nameof(c));
    }

    public static void Postfix(CyclopsFireSuppressionSystemButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            cyclops.BroadcastActivateFireSuppression(id);
        }
    }
}
