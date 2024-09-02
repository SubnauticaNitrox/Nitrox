using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using NitroxClient.Communication.Abstract;
namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FootstepSounds_OnStep_Patch : NitroxPatch, IDynamicPatch
{
    private const string EXO_STEP_SOUND_PATH = "event:/sub/exo/step";
    private const string PRECURSOR_STEP_SOUND_PATH = "event:/player/footstep_precursor_base";
    private const string METAL_STEP_SOUND_PATH = "event:/player/footstep_metal";
    

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
               ).MatchEndForward(new CodeMatch(OpCodes.Call, Reflect.Method((FMOD.Studio.EventInstance t) => t.release())),
                                                               new CodeMatch(OpCodes.Pop))
                                            .Advance(1)
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => SendFootstepPacket(default))))
                                            .InstructionEnumeration();
        /* event.release();
         * SendFootstepPacket(asset);
         * asset in this case is the FMOD asset of the footstep sound that was played
         * */
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
    public static void SendFootstepPacket(FMODAsset asset)
    {
        // Send footstep packet with the asset
        var localPlayer = Resolve<LocalPlayer>();
        if (localPlayer.PlayerId.HasValue)
        {
            FootstepPacket.StepSounds assetIndex;
            if (asset.path == PRECURSOR_STEP_SOUND_PATH)
            {
                assetIndex = FootstepPacket.StepSounds.PRECURSOR_STEP_SOUND;
            }
            else if (asset.path == METAL_STEP_SOUND_PATH)
            {
                assetIndex = FootstepPacket.StepSounds.METAL_STEP_SOUND;
            }
            else
            {
                assetIndex = FootstepPacket.StepSounds.LAND_STEP_SOUND;
            }
            var footstepPacket = new FootstepPacket(localPlayer.PlayerId.Value, assetIndex);
            Resolve<IPacketSender>().Send(footstepPacket);
        }
    }
}
