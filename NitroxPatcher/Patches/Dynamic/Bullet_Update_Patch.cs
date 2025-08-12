using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces local use of <see cref="Time.deltaTime"/> by <see cref="TimeManager.DeltaTime"/> and prevents remote bullets from detecting collisions
/// </summary>
public sealed partial class Bullet_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bullet t) => t.Update());

    /*
     * RaycastHit raycastHit;
     * REPLACE:
     * if (Physics.SphereCast(this.tr.position, this.shellRadius, this.tr.forward, out raycastHit, num, this.layerMask.value))
     * {
     *     num = raycastHit.distance;
     * BY:
     * if (!Bullet_Update_Patch.IsRemoteObject(this) && Physics.SphereCast(this.tr.position, this.shellRadius, this.tr.forward, out raycastHit, num, this.layerMask.value))
     * {
     *     num = raycastHit.distance;
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        Label label = generator.DefineLabel();

        // Replace the two occurences of Time.deltaTime
        return new CodeMatcher(instructions).ReplaceDeltaTime()
                                            .ReplaceDeltaTime()
                                            .MatchStartForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Call, Reflect.Property((Bullet t) => t.tr).GetGetMethod()),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Property((Transform t) => t.position).GetGetMethod()),
                                                new CodeMatch(OpCodes.Ldarg_0),
                                            ])
                                            // Skip the Ldarg_0 because it is the previous ifs' jump target
                                            .Advance(1)
                                            // Insert if (!Bullet_Update_Patch.IsRemoteObject(this)) before the condition
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => IsRemoteObject(default))),
                                                new CodeInstruction(OpCodes.Brtrue_S, label),
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                            ])
                                            // Find the destination of the position to go to
                                            .MatchStartForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Call, Reflect.Property((Bullet t) => t.tr).GetGetMethod()),
                                                new CodeMatch(OpCodes.Dup),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Property((Transform t) => t.position).GetGetMethod())
                                            ])
                                            .AddLabels([label])
                                            .InstructionEnumeration();
    }

    public static bool IsRemoteObject(Bullet bullet)
    {
        return bullet.GetComponent<BulletManager.RemotePlayerBullet>();
    }
}
