using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FootstepSounds_OnStep_Patch : NitroxPatch, IDynamicPatch
{
    private const string EXO_STEP_SOUND_PATH = "event:/sub/exo/step";

    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((FootstepSounds t) => t.OnStep(default));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
        From:

            evt.setVolume(volume);

        To:

            evt.setVolume(CalculateVolume(volume, this, asset, xform));
         */

        return new CodeMatcher(instructions)
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldloc_1),
                   new CodeMatch(OpCodes.Call, Reflect.Method((EventInstance evt) => evt.setVolume(default)))
               )
               .Insert(
                   new CodeInstruction(OpCodes.Ldarg_0),
                   new CodeInstruction(OpCodes.Ldloc_0),
                   new CodeInstruction(OpCodes.Ldarg_1),
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => CalculateVolume(default, default, default, default)))
               )
               .InstructionEnumeration();
    }

    // This method is called very often and should therefore be performant
    private static float CalculateVolume(float originalVolume, FootstepSounds instance, FMODAsset asset, Transform xform)
    {
        // For exosuits only "event:/sub/exo/step" is used inside FootstepSounds
        if (asset.path.Equals(EXO_STEP_SOUND_PATH, StringComparison.Ordinal) && (!Player.main.currentMountedVehicle || Player.main.currentMountedVehicle.gameObject != instance.gameObject))
        {
            // Origin is Exosuit which is controlled by remote player
            return FMODSystem.CalculateVolume(xform.position, Player.main.transform.position, stepSoundRadius.Value, originalVolume);
        }

        return originalVolume;
    }

    private static readonly Lazy<float> stepSoundRadius = new(() =>
    {
        Resolve<FMODWhitelist>().TryGetSoundData(EXO_STEP_SOUND_PATH, out SoundData soundData);
        return soundData.Radius;
    });
}
