using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Transfers <see cref="NitroxEntity"/> from a seed to the plant that is replacing it (the seed is being destroyed).
/// </summary>
public sealed partial class Planter_ReplaceItem_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter p) => p.ReplaceItem(default, default));

    public static void Postfix(Plantable seed, Plantable plant, bool __result)
    {
        if (!__result)
        {
            return;
        }

        if (seed.TryGetIdOrWarn(out NitroxId nitroxId))
        {
            NitroxEntity.SetNewId(plant.gameObject, nitroxId);
        }
    }
}
