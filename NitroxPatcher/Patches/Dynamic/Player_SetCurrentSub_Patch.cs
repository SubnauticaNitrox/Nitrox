using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Player_SetCurrentSub_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetCurrentSub", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Player __instance, SubRoot sub)
        {
            NitroxId subId = null;

            if (sub != null)
            {
                subId = NitroxEntity.GetId(sub.gameObject);
            }
            // When in the water of the moonpool, it can happen that you hammer change requests
            // while the sub is not changed. This will prevent that
            if (__instance.GetCurrentSub() != sub)
            {
                NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastSubrootChange(Optional.OfNullable(subId));
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
