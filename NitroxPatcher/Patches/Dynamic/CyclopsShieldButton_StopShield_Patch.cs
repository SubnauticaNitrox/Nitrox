using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsShieldButton_StopShield_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsShieldButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StopShield", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsShieldButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            // Shield is activated, if activeSprite is set as sprite
            bool isActive = (__instance.activeSprite == __instance.image.sprite);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeShieldState(id, isActive);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
