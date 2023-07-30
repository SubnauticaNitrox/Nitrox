using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When the player crafts items the game will leverage this API to select a pickupable
/// from their inventory and delete it.  We want to let the server know that the item
/// was successfully deleted.
/// </summary>
public sealed partial class ItemsContainer_DestroyItem_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((ItemsContainer t) => t.DestroyItem(default(TechType)));

    private static readonly InstructionsPattern removeItemPattern = new()
    {
        Ldarg_0,
        Ldarg_1,
        Reflect.Method((ItemsContainer container) => container.RemoveItem(default(TechType))),
        { Stloc_0, "NotifyServer" }
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        // After the call to RemoveItem (and storing the return value) we want to call our callback method
        return instructions.InsertAfterMarker(removeItemPattern, "NotifyServer", new CodeInstruction[]
        {
            new(Ldloc_0),
            new(Call, Reflect.Method(() => Callback(default)))
        });
    }

    private static void Callback(Pickupable pickupable)
    {
        if (pickupable.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }
}
