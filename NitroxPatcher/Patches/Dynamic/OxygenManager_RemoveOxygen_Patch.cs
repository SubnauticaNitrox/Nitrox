using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

/// <summary>
/// Prevents Local Player from loosing oxygen while he hasn't loaded yet
/// Oxygen removal starts after PlayerPositionInitialSyncProcessor. If player is moved in the water because before, he'll still be in the spawn pod
/// </summary>
namespace NitroxPatcher.Patches.Dynamic
{
    public class OxygenManager_RemoveOxygen_Patch : NitroxPatch, IDynamicPatch
    {
        private static MethodInfo TARGET_METHOD = Reflect.Method((OxygenManager t) => t.RemoveOxygen(default));

        public static bool Prefix(OxygenManager __instance)
        {
            return Multiplayer.Main.InitialSyncCompleted || Player.main.oxygenMgr != __instance;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
