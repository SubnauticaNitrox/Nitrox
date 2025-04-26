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
/// Syncs item deletion in trash can (under certain simulation ownership conditions, see <see cref="BroadcastDeletion"/>)
/// </summary>
public sealed partial class Trashcan_Update_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Trashcan t) => t.Update());

    /*
     * if (this.storageContainer.container.RemoveItem(item, true))
     * {
     *     BroadcastDeletion(this, item.gameObject);      <------- [INSERTED LINE]
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

    /// <remarks>
    /// Players only get simulation ownership on the base and on some objects, trashcan is not one of them
    /// So we are left with two cases:<br/>
    /// - if the trashcan is under a SubRoot we check simulation ownership onto it<br/>
    /// - else, we check simulation ownership on the trashcan and we hope at least someone has it
    /// </remarks>
    public static void BroadcastDeletion(Trashcan trashcan, GameObject gameObject)
    {
        if (!trashcan.TryGetNitroxId(out NitroxId trashcanId) ||
            !gameObject.TryGetNitroxId(out NitroxId objectId))
        {
            return;
        }

        if (trashcan.transform.parent && trashcan.transform.parent.GetComponent<SubRoot>())
        {
            if (trashcan.transform.parent.TryGetNitroxId(out NitroxId subRootId) &&
                Resolve<SimulationOwnership>().HasAnyLockType(subRootId))
            {
                Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
            }
            return;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(trashcanId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
        }
    }
}
