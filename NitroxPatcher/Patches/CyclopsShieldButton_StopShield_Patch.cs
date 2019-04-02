using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    class CyclopsShieldButton_StopShield_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsShieldButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StopShield", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsShieldButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            // Shield is activated, if activeSprite is set as sprite
            bool isActive = (__instance.activeSprite == __instance.image.sprite);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeShieldState(guid, isActive);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
