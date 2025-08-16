using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Synchronizes entities that Spawn something when they are killed, e.g. Coral Disks.
/// </summary>
public sealed partial class SpawnOnKill_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.RewriteOnPattern(
            [
                Reflect.Method(() => UnityEngine.Object.Instantiate(default(GameObject), default(Vector3), default(Quaternion))),
                Stloc_0,
                [
                    Ldarg_0,
                    Ldloc_0,
                    ((Action<SpawnOnKill, GameObject>)Callback).Method
                ],
                Ldarg_0
            ]
        );

    private static void Callback(SpawnOnKill spawnOnKill, GameObject spawningItem)
    {
        if (!spawnOnKill.TryGetNitroxEntity(out NitroxEntity destroyedEntity))
        {
            Log.Warn($"[{nameof(SpawnOnKill_OnKill_Patch)}] Could not find {nameof(NitroxEntity)} for breakable entity {spawnOnKill.gameObject.GetFullHierarchyPath()}.");
        }
        else
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntity.Id));
        }
        NitroxEntity.SetNewId(spawningItem, new());
        Resolve<Items>().Dropped(spawningItem);
    }
}
