using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    class Player_SetCurrentEscapePod_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly PropertyInfo TARGET_PROPERTY = TARGET_CLASS.GetProperty("currentEscapePod", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(ref Player __instance, EscapePod value)
        {
            NitroxId podId = null;

            if (value != null)
            {
                podId = NitroxEntity.GetId(value.gameObject);
            }
            // Why only send when podId is empty?
            // At the moment, SubrootId saves if a player is in an escape pod
            // Every time you leave an escape pod, BroadcastSubrootChange is called anyway, so this will reduce the load
            // and some bugs that can occur, due to both pod and base/cyclops use of the same field, will be suppressed
            if (podId != null)
            {
                NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastEscapePodChange(Optional.OfNullable(podId));
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {            
            PatchPrefix(harmony, TARGET_PROPERTY.GetSetMethod());
        }
    }
}
