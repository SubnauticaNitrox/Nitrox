using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class MapRoomFunctionality_UpdateCameraBlips_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomFunctionality mapRoom) => mapRoom.UpdateCameraBlips());

    public static void Postfix(MapRoomFunctionality __instance)
    {
        if (!__instance)
        {
            return;
        }

        PlayerManager playerManager = Resolve<PlayerManager>();
        ScannerRoomPlayerBlipManager.GetOrCreate(__instance, playerManager).RefreshNow();
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD, ((Action<MapRoomFunctionality>)Postfix).Method);
    }
}
