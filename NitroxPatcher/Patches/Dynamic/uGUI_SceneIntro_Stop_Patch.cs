using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SceneIntro_Stop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SceneIntro t) => t.Stop(default));

    public static bool Prefix(uGUI_SceneIntro __instance, bool isInterrupted)
    {
        // Those cases aren't caused by a normal skip so we need to let it work
        if ((XRSettings.enabled && VROptions.skipIntro) || !isInterrupted)
        {
            return true;
        }

        if (uGUI_SceneIntro_IntroSequence_Patch.IsWaitingForPartner)
        {
            // Stop is coming from skipping "Waiting for a partner to join" => play the cinematic normally.
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.SINGLEPLAYER);
            GameInput.ClearInput();
            ResetTimeDownForButton(GameInput.Button.UIMenu);
            uGUI_SceneIntro_IntroSequence_Patch.EnqueueStartCinematic(__instance);
            return false;
        }

        return true;
    }

    // Partial copied from GameInput.GetInputStateForButton()
    private static void ResetTimeDownForButton(GameInput.Button button)
    {
        for (int index1 = 0; index1 < GameInput.numDevices; ++index1)
        {
            for (int index2 = 0; index2 < GameInput.numBindingSets; ++index2)
            {
                int bindingInternal = GameInput.GetBindingInternal((GameInput.Device)index1, button, (GameInput.BindingSet)index2);
                if (bindingInternal != -1)
                {
                    GameInput.InputState inputState = GameInput.inputStates[bindingInternal];
                    inputState.flags = GameInput.InputStateFlags.Up;
                    inputState.timeDown = Time.unscaledTime + 1f; // 1 sec cooldown
                    GameInput.inputStates[bindingInternal] = inputState;
                }
            }
        }
    }
}
