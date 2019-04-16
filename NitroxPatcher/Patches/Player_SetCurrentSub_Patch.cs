using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    public class Player_SetCurrentSub_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetCurrentSub", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(SubRoot sub)
        {
            NitroxId subId = null;

            if (sub != null)
            {
                subId = NitroxIdentifier.GetId(sub.gameObject);
            }

            NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastSubrootChange(Optional<NitroxId>.OfNullable(subId));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
