using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsHelmHUDManager_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Update());

    public static void Postfix(CyclopsHelmHUDManager __instance)
    {
        // To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
        if (!__instance.hornObject.activeSelf && __instance.hudActive)
        {
            __instance.canvasGroup.interactable = false;
        }
        else if (!__instance.hudActive)
        {
            __instance.hudActive = true;
        }
        if (__instance.subLiveMixin.IsAlive())
        {
            if (__instance.motorMode.engineOn)
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOn");
            }
            else
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOff");
            }
        }
    }

    /*
     * Fix for #2525: vanilla computes
     *     int num = Mathf.CeilToInt(subRoot.powerRelay.GetPower() / subRoot.powerRelay.GetMaxPower() * 100f);
     * When all powercells are removed GetMaxPower() returns 0, so 0f / 0f is NaN and
     * Mathf.CeilToInt(NaN) is int.MinValue, making the HUD read "-2147483648%".
     * Clamp the result to a minimum of 0 so vanilla's own "{num}%" formatting renders "0%":
     *     int num = Mathf.Max(Mathf.CeilToInt(...), 0);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Call, Reflect.Method(() => UnityEngine.Mathf.CeilToInt(default(float)))))
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => UnityEngine.Mathf.Max(default(int), default(int)))))
                                            .InstructionEnumeration();
    }
}
