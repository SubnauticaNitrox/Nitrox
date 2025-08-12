//TODO: Find replacement for this, maybe the old ToggleLights_SetLightsActive_Patch?
#if SUBNAUTICA
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FlashLight_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FlashLight t) => t.onLightsToggled(default(bool)));

    public static void Postfix(FlashLight __instance, bool active)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new NitroxModel.Packets.ToggleLights(id, active));
        }
    }
}
#endif
