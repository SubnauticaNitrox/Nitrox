using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// The first step of OnHatched() is to check if the enzyme was attached, so that it can be destroyed.
/// Before this destruction occurs, let's let the server know that the item is being deleted.
/// </summary>
public sealed partial class Incubator_OnHatched_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Incubator t) => t.OnHatched());

    public static void Prefix(Incubator __instance)
    {
        if (__instance.enzymesObject.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }
}
