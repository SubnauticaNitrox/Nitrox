#if SUBNAUTICA
using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePodFirstUseCinematicsController_ReleaseCreature_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePodFirstUseCinematicsController t) => t.ReleaseCreature(default));

    /**
     * Avoid cinematics from spawning unsynced entites.
     * As soon as the cinematics are over, we'll kill them.
     */
    public static bool Prefix(GameObject creatureGO)
    {
        if (creatureGO)
        {
            UnityEngine.Object.Destroy(creatureGO);
        }

        return false;
    }

}
#endif
