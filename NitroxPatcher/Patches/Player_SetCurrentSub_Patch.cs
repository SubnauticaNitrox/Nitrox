using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches
{
    public class Player_SetCurrentSub_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetCurrentSub", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(SubRoot sub)
        {
            string subGuid = null;

            if (sub != null)
            {
                subGuid = GuidHelper.GetGuid(sub.gameObject);
            }

            NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastSubrootChange(Optional<string>.OfNullable(subGuid));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
