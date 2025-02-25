using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts destruction of GrownPlants differently than <see cref="LiveMixin_Kill_Patch"/> because
/// GrownPlants don't hold their own NitroxEntity
/// </summary>
public sealed partial class GrownPlant_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((GrownPlant p) => p.OnKill());

    public static void Prefix(GrownPlant __instance)
    {
        if (__instance.seed && __instance.seed.TryGetIdOrWarn(out NitroxId seedId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(seedId));
        }
    }
}
