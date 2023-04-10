using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes entities that Spawn something when they are killed, e.g. Coral Disks.
/// </summary>
public class SpawnOnKill_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnOnKill t) => t.OnKill());

    private static readonly InstructionsPattern spawnInstanceOnKillPattern = new()
    {
        Reflect.Method(() => Object.Instantiate(default(GameObject), default(Vector3), default(Quaternion))),
        { Stloc_0, "DropOnKillInstance" },
        Ldarg_0,
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        static IEnumerable<CodeInstruction> InsertCallbackCall(string label, CodeInstruction _)
        {
            switch (label)
            {
                case "DropOnKillInstance":
                    yield return new(Ldarg_0);
                    yield return new(Ldloc_0);
                    yield return new(Call, Reflect.Method(() => Callback(default, default)));
                    break;
            }
        }

        return instructions.Transform(spawnInstanceOnKillPattern, InsertCallbackCall);
    }

    private static void Callback(SpawnOnKill spawnOnKill, GameObject spawningItem)
    {
        if (!NitroxEntity.TryGetEntityFrom(spawnOnKill.gameObject, out NitroxEntity destroyedEntity))
        {
            Log.Warn($"[{nameof(BreakableResource_BreakIntoResources_Patch)}] Could not find {nameof(NitroxEntity)} for breakable entity {spawnOnKill.gameObject.GetFullHierarchyPath()}.");
        } else
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntity.Id));
        }
        NitroxEntity.SetNewId(spawningItem, new());
        Resolve<Items>().Dropped(spawningItem);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
