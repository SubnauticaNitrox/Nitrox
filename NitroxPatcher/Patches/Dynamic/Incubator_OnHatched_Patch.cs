using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// The first step of OnHatched() is to check if the enzyme was attached, so that it can be destroyed.
/// Before this destruction occurs, let's let the server know that the item is being deleted. 
/// </summary>
public class Incubator_OnHatched_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Incubator t) => t.OnHatched());

    public static void Prefix(Incubator __instance)
    {
        if (__instance.enzymesObject)
        {
            NitroxId id = NitroxEntity.GetId(__instance.enzymesObject);
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
