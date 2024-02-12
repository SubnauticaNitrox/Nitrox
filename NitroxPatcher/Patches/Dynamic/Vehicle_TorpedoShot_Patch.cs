using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_TorpedoShot_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Vehicle.TorpedoShot(default, default, default));

    /*
     * Inserts a DUP instruction after the SeamothTorpedo object is pushed onto the stack so we can use it later on
     * in the inserted callback.
     * NB: Ldarg_0 refers to the 1st parameter (ItemsContainer) and Ldarg_1 refers to the 2nd parameter (TorpedoType) as this method is static
     * 
     * MODIFICATION:
     * component.Shoot(muzzle.position, aimingTransform.rotation, num, -1f);
     * Vehicle_TorpedoShot_Patch.TorpedoShotCallback(dupped object, torpedoType);    <--- INSERTED LINE
     * return true;
     * 
     * "dupped object" is the SeamothTorpedo object from the line:
     * gameObject.GetComponent<SeamothTorpedo>();
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Always match for one more instruction after the searched one because the cursor will be moved right before it
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Pop),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((GameObject t) => t.GetComponent<SeamothTorpedo>())),
                                                new CodeMatch(OpCodes.Call),
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Dup),
                                            ])
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((Bullet t) => t.Shoot(default, default, default, default))),
                                                new CodeMatch(OpCodes.Ldc_I4_1),
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_1),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => TorpedoShotCallback(default, default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void TorpedoShotCallback(SeamothTorpedo seamothTorpedo, TorpedoType torpedoType)
    {
        NitroxId bulletId = NitroxEntity.GenerateNewId(seamothTorpedo.gameObject);

        NitroxVector3 position = seamothTorpedo.transform.position.ToDto();
        NitroxQuaternion rotation = seamothTorpedo.transform.rotation.ToDto();

        // In Bullet.Shoot, _consumption = f(lifeTime), lifeTime = g(_consumption), this is g:
        float lifeTime = seamothTorpedo._consumption > 0 ? 1f / seamothTorpedo._consumption : 0f;

        Resolve<IPacketSender>().Send(new TorpedoShot(bulletId, torpedoType.techType.ToDto(), position, rotation, seamothTorpedo.speed, lifeTime));
    }
}
