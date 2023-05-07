#if SUBNAUTICA
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Seaglide_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Seaglide t) => t.onLightsToggled(default(bool)));

    public static void Postfix(Seaglide __instance, bool active)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new NitroxModel.Packets.ToggleLights(id, active));
        }
    }
}
#endif
