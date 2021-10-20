using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class Player_SetCurrentEscapePod_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly PropertyInfo TARGET_PROPERTY = Reflect.Property((Player p) => p.currentEscapePod);

        public static void Prefix(ref Player __instance, EscapePod value)
        {
            NitroxId podId = null;

            if (value != null)
            {
                podId = NitroxEntity.GetId(value.gameObject);
            }
            NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastEscapePodChange(Optional.OfNullable(podId));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_PROPERTY.GetSetMethod());
        }
    }
}
