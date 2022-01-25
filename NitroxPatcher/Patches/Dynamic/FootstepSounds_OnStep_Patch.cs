using System.Reflection;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class FootstepSounds_OnStep_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FootstepSounds t) => t.OnStep(default(Transform)));
    private static SoundData? stepSoundData;

    public static bool Prefix(FootstepSounds __instance, Transform xform)
    {
        // We want to manage it when the exosuit playing this sound is not controlled by the current player
        if (Player.main.currentMountedVehicle && Player.main.currentMountedVehicle.gameObject == __instance.gameObject)
        {
            return true;
        }
        // Make the modifications to the base code so that it plays in 3D
        if (__instance.ShouldPlayStepSounds())
        {
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
                if (Resolve<FMODSystem>().HasSoundData(asset.path, out SoundData assetSoundData))
                {
                    stepSoundData = assetSoundData;
                }
                else
                {
                    Log.Warn($"[FootstepSounds_OnStep_Patch] Couldn't find sound data for asset path: {asset.path}");
                }
            }

            RecalculateVolume(xform, out float volume);
            EventInstance @event = FMODUWE.GetEvent(asset);
            if (@event.isValid())
            {
                if (__instance.fmodIndexSpeed < 0)
                {
                    __instance.fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(@event, "speed");
                }
                
                ATTRIBUTES_3D attributes = xform.To3DAttributes();
                @event.set3DAttributes(attributes);
                @event.setParameterValueByIndex(__instance.fmodIndexSpeed, __instance.groundMoveable.GetVelocity().magnitude);
                @event.setVolume(volume);
                @event.start();
                @event.release();
            }
        }
        return false;
    }

    private static void RecalculateVolume(Transform xform, out float volume)
    {
        float radius = stepSoundData?.SoundRadius ?? 50f;
        float distance = xform.position.ToDto().Distance(Player.main.transform.position.ToDto());
        volume = PhysicsHelper.CalculateVolume(distance, radius, 1f);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
