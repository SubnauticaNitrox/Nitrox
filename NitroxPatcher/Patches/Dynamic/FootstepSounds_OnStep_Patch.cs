using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class FootstepSounds_OnStep_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FootstepSounds t) => t.OnStep(default(Transform)));
    private static SoundData? stepSoundData;

    public static bool Prefix(FootstepSounds __instance, Transform xform)
    { 
        // We want to manage it only when an exosuit is playing the sound and that this exosuit is not controlled by the current player
        if (__instance.GetComponent<Player>() || (Player.main.currentMountedVehicle && Player.main.currentMountedVehicle.gameObject == __instance.gameObject))
        {
            return true;
        }
        // Make the modifications to the base code so that the volume is updated depending on the distance
        if (!__instance.ShouldPlayStepSounds())
        {
            return false;
        }
        FMODAsset asset;
        if (Player.main.precursorOutOfWater)
        {
            asset = __instance.precursorInteriorSound;
        }
        else if ((__instance.groundMoveable.GetGroundSurfaceType() == VFXSurfaceTypes.metal || Player.main.IsInside() || Player.main.GetBiomeString() == FootstepSounds.crashedShip) && Player.main.currentWaterPark == null)
        {
            asset = __instance.metalSound;
        }
        else
        {
            asset = __instance.landSound;
        }

        if (!stepSoundData.HasValue)
        {
            if (Resolve<FMODSystem>().TryGetSoundData(asset.path, out SoundData assetSoundData))
            {
                stepSoundData = assetSoundData;
            }
            else
            {
                Log.WarnOnce($"[FootstepSounds_OnStep_Patch] Couldn't find sound data for asset path: {asset.path}");
            }
        }

        RecalculateVolume(xform, out float volume);
        EventInstance evt = FMODUWE.GetEvent(asset);
        if (evt.isValid())
        {
            if (__instance.fmodIndexSpeed < 0)
            {
                __instance.fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(evt, "speed");
            }

            evt.setParameterValueByIndex(__instance.fmodIndexSpeed, __instance.groundMoveable.GetVelocity().magnitude);
            evt.setVolume(volume);
            evt.start();
            evt.release();
        }
        return false;
    }

    private static void RecalculateVolume(Transform xform, out float volume)
    {
        float radius = stepSoundData?.SoundRadius ?? 50f;
        float distance = Vector3.Distance(xform.position, Player.main.transform.position);
        volume = SoundHelper.CalculateVolume(distance, radius, 1f);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
