using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs item deletion in trash can (only if you have simulation ownership over the trash can)
/// </summary>
public sealed partial class Trashcan_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Trashcan t) => t.Update());

    /*
     * if (this.storageContainer.container.RemoveItem(item, true))
     * {
     *     BroadcastDeletion(item.gameObject);      <------- [INSERTED LINE]
     *     UnityEngine.Object.Destroy(item.gameObject);
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).End()
                                            .Insert([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_1),
                                                new CodeInstruction(OpCodes.Callvirt, Reflect.Property((Component t) => t.gameObject).GetGetMethod()),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastDeletion(default, default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void BroadcastDeletion(Trashcan trashcan, GameObject gameObject)
    {
        if (trashcan.TryGetNitroxId(out NitroxId trashcanId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(trashcanId) &&
            gameObject.TryGetNitroxId(out NitroxId objectId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
        }
    }
}
