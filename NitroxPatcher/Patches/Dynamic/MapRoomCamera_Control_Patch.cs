using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class MapRoomCamera_ControlCamera_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.ControlCamera(default(Player), default(MapRoomScreen)));
        private static CameraControlManager cameraControlManager;

        public static void Postfix(MapRoomCamera __instance)
        {
            cameraControlManager ??= Resolve<CameraControlManager>();
            cameraControlManager.ControlCamera(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

