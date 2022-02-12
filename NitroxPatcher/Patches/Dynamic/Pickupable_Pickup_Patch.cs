using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Pickupable_Pickup_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Pickupable p) => p.Pickup(default(bool)));

        public static bool Prefix(Pickupable __instance)
        {
            NitroxServiceLocator.LocateService<Item>().PickedUp(__instance.gameObject, __instance.GetTechType());
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

