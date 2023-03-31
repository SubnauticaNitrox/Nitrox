using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When the player crafts items the game will leverage this API to select a pickupable
/// from their inventory and delete it.  We want to let the server know that the item
/// was successfully deleted. 
/// </summary>
public class ItemsContainer_DestroyItem_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((ItemsContainer t) => t.DestroyItem(default(TechType)));

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Stloc_0;

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            // Injects:
            //   
            //  Pickupable pickupable = this.RemoveItem(techType);
            //  >>> Callback(pickupable);
            //
            if (instruction.opcode.Equals(INJECTION_OPCODE))
            {
                yield return new(OpCodes.Ldloc_0);
                yield return new(OpCodes.Call, Reflect.Method(() => Callback(default)));
            }
        }
    }

    public static void Callback(Pickupable pickupable)
    {
        if (pickupable)
        {
            NitroxId id = NitroxEntity.GetId(pickupable.gameObject);
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
