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

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
From:
    bool lostStuff = Inventory.main.LoseItems();
    ........
    ErrorMessage.AddWarning((!lostStuff) ? Language.main.Get("YouDied") : Language.main.Get("YouDiedLostStuff"));
To:
    //bool lostStuff = Inventory.main.LoseItems(); [REPLACED]
    LoseItemsIfKeepInventoryDisabled() [NEW]
    ........
    //ErrorMessage.AddWarning((!lostStuff) ? Language.main.Get("YouDied") : Language.main.Get("YouDiedLostStuff")); [REMOVED]
 */
        return new CodeMatcher(instructions).MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldsfld),
            new CodeMatch(OpCodes.Callvirt, Reflect.Method((Inventory t) => t.LoseItems())))
            // Remove the call for the Inventory.LoseItems function
            .RemoveInstructions(4)
            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => LoseItemsIfKeepInventoryDisabled())))
            .Advance(118)
            // Remove the ErrorMessage.AddWarning((!lostStuff) ? Language.main.Get("YouDied") : Language.main.Get("YouDiedLostStuff")) line
            .RemoveInstructions(11)
            .InstructionEnumeration();
    }

    private static void LoseItemsIfKeepInventoryDisabled()
    {
        if (Resolve<LocalPlayer>().KeepInventory == false)
        {
            if (Inventory.main.LoseItems())
            {
                ErrorMessage.AddWarning(Language.main.Get("YouDiedLostStuff"));
            }
            else
            {
                ErrorMessage.AddWarning(Language.main.Get("YouDied"));
            }
        }
        else
        {
            ErrorMessage.AddWarning(Language.main.Get("YouDied"));
        }
    }
}
