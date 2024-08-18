// TODO: Rewrite for new footstep system
#if SUBNAUTICA
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FootstepSounds_OnStep_Patch : NitroxPatch, IDynamicPatch
{
    private const string PRECURSOR_STEP_SOUND_PATH = "event:/player/footstep_precursor_base";
    private const string METAL_STEP_SOUND_PATH = "event:/player/footstep_metal";
    private const string LAND_STEP_SOUND_PATH = "event:/player/footstep_dirt";

    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((FootstepSounds t) => t.OnStep(default));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
        From:
            evt.setVolume(volume);

        To:
            evt.setVolume(CalculateVolume(volume, this, asset, xform));


        From:
            event.release();

        To:
            event.release();
            SendFootstepPacket(asset);
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
               .MatchEndForward(
                   new CodeMatch(OpCodes.Call, Reflect.Method((EventInstance evt) => evt.release())),
                   new CodeMatch(OpCodes.Pop)
               )
               .Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
               .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => SendFootstepPacket(default))))
               .InstructionEnumeration();
    }

    // This method is called very often and should therefore be performant
    private static float CalculateVolume(float originalVolume, FootstepSounds instance, FMODAsset asset, Transform xform)
    {
        // For exosuits only "event:/sub/exo/step" is used inside FootstepSounds
        if (asset.path.Equals(LAND_STEP_SOUND_PATH, StringComparison.Ordinal) && (!Player.main.currentMountedVehicle || Player.main.currentMountedVehicle.gameObject != instance.gameObject))
        {
            // Origin is Exosuit which is controlled by remote player
            return FMODSystem.CalculateVolume(xform.position, Player.main.transform.position, stepSoundRadius.Value, originalVolume);
        }

        return originalVolume;
    }

    private static readonly Lazy<float> stepSoundRadius = new(() =>
    {
        Resolve<FMODWhitelist>().TryGetSoundData(LAND_STEP_SOUND_PATH, out SoundData soundData);
        return soundData.Radius;
    });

    private static void SendFootstepPacket(FMODAsset asset)
    {
        if (!Resolve<LocalPlayer>().PlayerId.HasValue)
        {
            return;
        }
        FootstepPacket.StepSounds assetIndex;
        switch (asset.path)
        {
            case PRECURSOR_STEP_SOUND_PATH:
                assetIndex = FootstepPacket.StepSounds.PRECURSOR;
                break;
            case METAL_STEP_SOUND_PATH:
                assetIndex = FootstepPacket.StepSounds.METAL;
                break;
            case LAND_STEP_SOUND_PATH:
                assetIndex = FootstepPacket.StepSounds.LAND;
                break;
            default:
                return;
        }
        FootstepPacket footstepPacket = new(Resolve<LocalPlayer>().PlayerId.Value, assetIndex);
        Resolve<IPacketSender>().Send(footstepPacket);
    }
}
#endif
