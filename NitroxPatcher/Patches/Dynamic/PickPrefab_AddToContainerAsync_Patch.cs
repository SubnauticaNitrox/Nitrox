using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Sync destruction of giver object when required.
/// This code is only executed from a local input so we don't need to check for ownership.
/// </summary>
public sealed partial class PickPrefab_AddToContainerAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((PickPrefab t) => t.AddToContainerAsync(default, default)));

    /*
     * 1st injection:
     * if (!component)
     * {
     *     UnityEngine.Object.Destroy(gameObject);
     *     BroadcastDeletion(this);      <------- [INSERTED LINE]
     *  
     *  2nd injection:
     *  else if (!container.HasRoomFor(component))
     *  {
     *     UnityEngine.Object.Destroy(gameObject);
     *     BroadcastDeletion(this);      <------- [INSERTED LINE]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // The 2 injections are similar (looking for a destroy instruction and adding our callback after it)
        return new CodeMatcher(instructions).MatchEndForward(new CodeMatch(OpCodes.Call, Reflect.Method(() => Object.Destroy(default))))
                                            .Repeat(matcher =>
                                            {
                                                matcher.Advance(1)
                                                       .InsertAndAdvance([
                                                           new CodeInstruction(OpCodes.Ldarg_0),
                                                           new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastDeletion(default)))
                                                       ]);
                                            }).InstructionEnumeration();
    }

    public static void BroadcastDeletion(PickPrefab pickPrefab)
    {
        if (pickPrefab.TryGetNitroxId(out NitroxId objectId) ||
            (pickPrefab.TryGetComponent(out GrownPlant grownPlant) && grownPlant.seed && grownPlant.seed.TryGetNitroxId(out objectId)))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
        }
    }
}
