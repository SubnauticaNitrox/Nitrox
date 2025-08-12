using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Registers knife hits's dealer as the main Player object
/// </summary>
public sealed partial class Knife_OnToolUseAnim_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Knife t) => t.OnToolUseAnim(default));
#if SUBNAUTICA
    /*
     * 
     * bool flag = liveMixin.IsAlive();
     * REPLACE below line
     * liveMixin.TakeDamage(this.damage, vector, this.damageType, null);
     * 
     * WITH:
     * liveMixin.TakeDamage(this.damage, vector, this.damageType, Player.mainObject);
     * this.GiveResourceOnDamage(gameObject, liveMixin.IsAlive(), flag);
     * 
     */
#elif BELOWZERO
    /*
     *
     * bool flag = liveMixin.IsAlive();
     * REPLACE below line
     * liveMixin.TakeDamage(this.damage, position, this.damageType, global::Utils.GetLocalPlayer());
     *
     * WITH:
     * liveMixin.TakeDamage(this.damage, position, this.damageType, Player.mainObject);
     * this.GiveResourceOnDamage(gameObject, liveMixin.IsAlive(), flag);
     *
     */
#endif
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
#if SUBNAUTICA
                                                new CodeMatch(OpCodes.Ldloc_0),
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldfld),
                                                new CodeMatch(OpCodes.Ldnull)
#elif BELOWZERO
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldfld),
                                                new CodeMatch(OpCodes.Call)
#endif
                                            ])
                                            .Set(OpCodes.Ldsfld, Reflect.Field(() => Player.mainObject))
                                            .InstructionEnumeration();
    }
}
