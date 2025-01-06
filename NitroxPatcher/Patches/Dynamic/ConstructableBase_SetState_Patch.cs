using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ConstructableBase_SetState_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ConstructableBase t) => t.SetState(default, default));

    /*
     * Make it become
     * if (Builder.CanDestroyObject(gameObject))
     * {
     *     ConstructableBase_SetState_Patch.BeforeDestroy(gameObject); <==========
     *     UnityEngine.Object.Destroy(gameObject);
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.RewriteOnPattern([
            Reflect.Method(() => Builder.CanDestroyObject(default)),
            Brfalse,
            [
                TARGET_METHOD.Ldloc<GameObject>(),
                Reflect.Method(() => BeforeDestroy(default))
            ]
        ]);

    public static void BeforeDestroy(GameObject gameObject)
    {
        if (gameObject && gameObject.TryGetNitroxId(out NitroxId nitroxId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(nitroxId));
        }
    }
}
