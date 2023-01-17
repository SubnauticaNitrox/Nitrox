using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class Constructor_Deploy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructor t) => t.Deploy(default(bool)));

    public static void Prefix(Constructor __instance, bool value)
    {
        // only trigger updates when there is a valid state change.
        if(value != __instance.deployed)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
