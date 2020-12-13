using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;

namespace Nitrox.Patcher.Patches.Dynamic
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
            NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastEscapePodChange(Optional.OfNullable(podId));
        }

        public override void Patch(HarmonyInstance harmony)
        {            
            PatchPrefix(harmony, TARGET_PROPERTY.GetSetMethod());
        }
    }
}
