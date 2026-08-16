using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class FlashLight_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FlashLight t) => t.onLightsToggled(default(bool)));

    public static void Postfix(FlashLight __instance, bool active)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new Nitrox.Model.Subnautica.Packets.ToggleLights(id, active));
        }
    }
}
