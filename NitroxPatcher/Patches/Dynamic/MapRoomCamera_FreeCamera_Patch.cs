using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class MapRoomCamera_FreeCamera_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.FreeCamera(default(bool)));
        private static CameraControlManager cameraControlManager;

        public static void Postfix()
        {
            cameraControlManager ??= Resolve<CameraControlManager>();
            cameraControlManager.FreeCamera();
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
