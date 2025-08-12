using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ToggleLights_SetLightsActive_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ToggleLights t) => t.SetLightsActive(default(bool)));

    public static readonly InstructionsPattern PlayEnvSoundPattern = new(2)
    {
        { new() { OpCode = OpCodes.Call, Operand = new(nameof(Utils), nameof(Utils.PlayEnvSound)) }, "ReplacePlayEnvSound" },
    };

    public static readonly InstructionsPattern PlayFMODAssetPattern = new(2)
    {
        { new() { OpCode = OpCodes.Call, Operand = new(nameof(Utils), nameof(Utils.PlayFMODAsset)) }, "ReplacePlayFMODAsset" },
    };

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Replace  Utils.PlayEnvSound(arg1, arg2, arg3) with PlayEnvSound3D(arg1, arg2, arg3)
        // and also Utils.PlayFMODAsset(arg1, arg2, arg3) with PlayFMODAsset3D(arg1, arg2, arg3)
        return instructions.Transform(PlayEnvSoundPattern, (label, instruction) =>
        {
            if (label.Equals("ReplacePlayEnvSound"))
            {
                instruction.operand = Reflect.Method(() => PlayEnvSound3D(default, default, 0));
            }
        }).Transform(PlayFMODAssetPattern, (label, instruction) =>
        {
            if (label.Equals("ReplacePlayFMODAsset"))
            {
                instruction.operand = Reflect.Method(() => PlayFMODAsset3D(default, default, 0));
            }
        });
    }

    private static void PlayEnvSound3D(FMOD_StudioEventEmitter eventEmitter, Vector3 position, float soundRadiusObsolete)
    {
        string assetPath = eventEmitter.asset ? eventEmitter.asset.path : eventEmitter.path;
        if (!Resolve<FMODWhitelist>().TryGetSoundData(assetPath, out SoundData eventEmitterSoundData))
        {
            Log.Error($"Couldn't find {assetPath} inside {nameof(FMODWhitelist)}. Not playing this sound.");
            return;
        }

        float volume = FMODSystem.CalculateVolume(position, Player.main.transform.position, eventEmitterSoundData.Radius, 1f);
        if (volume > 0)
        {
            eventEmitter.PlayOneShotNoWorld(position, volume);
        }
    }

    private static void PlayFMODAsset3D(FMODAsset asset, Transform transform, float soundRadiusObsolete)
    {
        if (!Resolve<FMODWhitelist>().TryGetSoundData(asset.path, out SoundData fmodAssetSoundData))
        {
            Log.Error($"Couldn't find {asset.path} inside {nameof(FMODWhitelist)}. Not playing sound.");
            return;
        }

        float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, fmodAssetSoundData.Radius, 1f);
        if (volume > 0)
        {
            FMODUWE.PlayOneShot(asset, transform.position, volume);
        }
    }
}
