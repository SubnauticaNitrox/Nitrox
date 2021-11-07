using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Player_EnterLockedMode_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.EnterLockedMode(default(Transform), default(bool)));
        private static LocalPlayer localPlayer;

        public static void Postfix(Transform parent, bool teleport)
        {
            localPlayer ??= Resolve<LocalPlayer>();
            if (parent == null && teleport == false)
            {
                // When a player enters a locked state, we should stop all placeholder movement automatically created by the other clients
                // e.g. you clicked on the MapRoomScreen and entered the camera controlling but you were moving so other clients consider that you're still running in that direction (you go through the world in their perspective)
                // Doesn't seem to work as it should
                Vector3 currentPosition = Player.main.transform.position;
                Quaternion bodyRotation = MainCameraControl.main.viewModel.transform.rotation;
                localPlayer.UpdateLocation(currentPosition, Vector3.zero, bodyRotation, Quaternion.identity, Optional.Empty);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
