using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseAddModuleGhost_SetupGhost_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseAddModuleGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetupGhost", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(BaseAddModuleGhost __instance)
        {

            if (NitroxServiceLocator.LocateService<Building>().isInitialSyncing || NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_SetupGhost_Pre");
#endif

                __instance.ReflectionCall("UpdateSize", false, false, new object[] { Int3.one });
                __instance.ReflectionSet("direction", Base.Direction.North);
                __instance.ReflectionSet("directions", new List<Base.Direction>(Base.HorizontalDirections));

                return false;
            }
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
