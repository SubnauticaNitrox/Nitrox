using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Inventory_LoseItems_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Inventory t) => t.LoseItems());

    /*
     * if (this.InternalDropItem(list[i].item, false))
     * {
     *     BroadcastItemDropped(list[i].item);      <--------- [INSERTED LINE]
     *     flag = true;
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Call, Reflect.Method((Inventory t) => t.InternalDropItem(default, default))),
                                                new CodeMatch(OpCodes.Brfalse),
                                                new CodeMatch(OpCodes.Ldc_I4_1),
                                                new CodeMatch(OpCodes.Stloc_1),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldloc_0), // Get "list" reference
                                                TARGET_METHOD.Ldloc<int>(), // Get "i" reference
                                                new CodeInstruction(OpCodes.Callvirt, Reflect.Method((List<InventoryItem> t) => t[default])), // Get item at index "i" from "list"
                                                new CodeInstruction(OpCodes.Callvirt, Reflect.Property((InventoryItem t) => t.item).GetGetMethod()), // Get reference from InventoryItem.item
                                                new CodeInstruction(OpCodes.Callvirt, Reflect.Method(() => BroadcastItemDropped(default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void BroadcastItemDropped(Pickupable pickupable)
    {
        Rigidbody rigidbody = pickupable.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Resolve<Items>().Dropped(pickupable.gameObject);
    }
}
