using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SceneIntro_HandleInput_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SceneIntro t) => ((IInputHandler)t).HandleInput());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /* Removes uGUI_SceneIntro.Stop() and uses our HandleButtonHeld feedback as return value
         *
         * Replaces:
         *      this.Stop(true);
         *      return false;
         *
         * With:
         *      return uGUI_SceneIntro_HandleInput_Patch.HandleButtonHeld(this);
         */
        return new CodeMatcher(instructions)
               .MatchStartForward(
                   new CodeMatch(OpCodes.Ldc_I4_1),
                   new CodeMatch(OpCodes.Call, Reflect.Method((uGUI_SceneIntro si) => si.Stop(default))),
                   new CodeMatch(OpCodes.Ldc_I4_0),
                   new CodeMatch(OpCodes.Ret)
               )
               .RemoveInstructions(3)
               .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => HandleButtonHeld(default))))
               .InstructionEnumeration();
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool HandleButtonHeld(uGUI_SceneIntro instance)
    {
        if (uGUI_SceneIntro_IntroSequence_Patch.IsWaitingForPartner) // Skipping waiting for player
        {
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.SINGLEPLAYER);
            GameInput.ClearInput();
            ResetTimeDownForButton(GameInput.Button.UIMenu);
            uGUI_SceneIntro_IntroSequence_Patch.EnqueueStartCinematic(instance);
            return true;
        }

        if (!uGUI_SceneIntro_IntroSequence_Patch.IsWaitingForPartner &&
            Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.SINGLEPLAYER) // Skipping intro alone
        {
            uGUI_SceneIntro_IntroSequence_Patch.SkipLocalCinematic(instance, true);
            return false;
        }

        Log.Error($"Undefined behaviour occured inside {nameof(uGUI_SceneIntro_HandleInput_Patch)}");
        return false;
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
