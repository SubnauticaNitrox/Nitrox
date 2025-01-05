using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;
public sealed partial class Player_ResetPlayerOnDeath_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Player t) => t.ResetPlayerOnDeath(default)));
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        /*
From:
    bool lostStuff = Inventory.main.LoseItems();
To:
    //bool lostStuff = Inventory.main.LoseItems();
    LoseItemsIfKeepInventoryEnabled(Inventory.main)
 */
        return new CodeMatcher(instructions).MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldsfld),
            new CodeMatch(OpCodes.Callvirt, Reflect.Method((Inventory t) => t.LoseItems())))
            // Skip the 
            .Advance(1).
            InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0)).
            Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => LoseItemsIfKeepInventoryEnabled(default))))
            .InstructionEnumeration();
    }
    public static void LoseItemsIfKeepInventoryEnabled(Inventory inventory)
    {
        if (Resolve<LocalPlayer>().KeepInventory) {
            return;
        }
        inventory.LoseItems();
    }
}
