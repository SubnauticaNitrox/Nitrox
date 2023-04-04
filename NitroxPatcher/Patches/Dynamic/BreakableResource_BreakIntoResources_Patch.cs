using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroxPatcher.Patches.Dynamic;

public class BreakableResource_BreakIntoResources_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((BreakableResource t) => t.BreakIntoResources());

    public static void Prefix(BreakableResource __instance)
    {
        // Gets the entity ID
        NitroxId id = NitroxEntity.GetId(__instance.gameObject);
        // Send packet to destroy the entity
        Resolve<IPacketSender>().Send(new EntityDestroyed(id));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
