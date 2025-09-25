using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents chargers from working on remote clients since battery charging is synced directly on charge property modification (<see cref="Battery_charge_set_Patch"/>).
/// This works by postponing charge attempts again and again as long we don't have simulation ownership.
/// </summary>
public sealed partial class Charger_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Charger t) => t.Update());

    /*
     * bool flag = false;
     * Charger_Update_Patch.CheckSimulation(this); <--- [INSERTED LINE]
     * if (this.nextChargeAttemptTimer <= 0f)
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Stfld, Reflect.Field((Charger t) => t.nextChargeAttemptTimer)),
                                                new CodeMatch(OpCodes.Ldc_I4_0),
                                                new CodeMatch(OpCodes.Stloc_0),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => CheckSimulation(default)))
                                            ]).InstructionEnumeration();
    }

    public static void CheckSimulation(Charger charger)
    {
        // Only check when attempting to charge
        if (charger.nextChargeAttemptTimer > 0f)
        {
            return;
        }

        // In case we don't locally simulate the SubRoot responsible for this charger, we postpone the next charge attempt
        if (charger.transform.parent.TryGetNitroxId(out NitroxId parentId) &&
            !Resolve<SimulationOwnership>().HasAnyLockType(parentId))
        {
            // 5 seconds is the regular value between two attempts (see Update)
            charger.nextChargeAttemptTimer = 5f;

            // Copied from Update to ensure UI is still updated
            charger.ToggleUIPowered(true);
            foreach (KeyValuePair<string, IBattery> entry in charger.batteries)
            {
                IBattery battery = entry.Value;
                if (battery != null && charger.slots.TryGetValue(entry.Key, out Charger.SlotDefinition slotDefinition))
                {
                    charger.UpdateVisuals(slotDefinition, battery.charge / battery.capacity);
                }
            }
        }
    }
}
