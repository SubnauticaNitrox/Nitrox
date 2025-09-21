using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.OnHandClick(default));

    public static void Postfix(BulkheadDoor __instance)
    {
        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        bool doorWasOpen = __instance.opened;
        Resolve<IPacketSender>().Send(new BulkheadDoorStateChanged(id, doorWasOpen));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD, ((Action<BulkheadDoor>)Postfix).Method);
    }
}
