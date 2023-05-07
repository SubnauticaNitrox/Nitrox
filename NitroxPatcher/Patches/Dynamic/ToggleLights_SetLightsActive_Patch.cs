#if BELOWZERO
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ToggleLights_SetLightsActive_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ToggleLights t) => t.SetLightsActive(default(bool)));

    public static void Postfix(ToggleLights __instance, bool active)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new NitroxModel.Packets.ToggleLights(id, active));
        }
    }
}
#endif
