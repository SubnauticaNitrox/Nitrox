using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the destruction of a Flare once it's out of energy if the local player simulates it.
/// </summary>
public sealed partial class Flare_OnDestroy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.OnDestroy());
    
    public static void Prefix(Flare __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId flareId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(flareId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(flareId));
        }
    }
}
