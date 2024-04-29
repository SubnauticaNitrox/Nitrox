using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs egg deletion when hatching.
/// </summary>
public sealed partial class CreatureEgg_Hatch_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureEgg t) => t.Hatch());

    public static void Postfix(CreatureEgg __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId eggId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(eggId))
        {
            return;
        }

        Resolve<IPacketSender>().Send(new EntityDestroyed(eggId));
    }
}
