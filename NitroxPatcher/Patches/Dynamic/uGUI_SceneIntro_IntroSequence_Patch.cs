using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SceneIntro_IntroSequence_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = AccessTools.EnumeratorMoveNext(Reflect.Method((uGUI_SceneIntro si) => si.IntroSequence()));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
               // Override AnyKeyDown check
               .MatchEndForward(
                   new CodeMatch(OpCodes.Call, Reflect.Method(() => GameInput.AnyKeyDown()))
               )
               .SetOperandAndAdvance(Reflect.Method(() => AnyKeyDownOrModeCompleted()))
               // Insert custom check if cinematic should be started => waiting for other player & enable skip functionality
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldfld, Reflect.Field((uGUI_SceneIntro si) => si.moveNext)),
                   new CodeMatch(OpCodes.Brfalse))
               .Insert(
                   new CodeInstruction(OpCodes.Ldloc_1),
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => IsRemoteCinematicReady(default))),
                   new CodeInstruction(OpCodes.And)
               )
               // Run our prepare code when cinematic starts
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => EscapePod.main)),
                   new CodeMatch(OpCodes.Callvirt, Reflect.Method(() => EscapePod.main.TriggerIntroCinematic()))
               )
               .Advance(1)
               .Insert(
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => StartRemoteCinematic()))
               )
               // Disable cinematic skip when cinematic already started in duo mode
               .MatchStartForward(
                   new CodeMatch(OpCodes.Call, Reflect.Property(() => Time.time).GetMethod)
               )
               .SetOperandAndAdvance(Reflect.Method(() => GetSkipTime()))
               .Advance(1)
               .Insert(
                   new CodeInstruction(OpCodes.Ldloc_1),
                   new CodeInstruction(OpCodes.Ldfld, Reflect.Field((uGUI_SceneIntro si) => si.skipText)),
                   new CodeInstruction(OpCodes.Ldc_R4, 0f),
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => TMProExtensions.SetAlpha(default, default(float))))
               )
               // Replace intro text
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldstr, "IntroUWEPresents")
               )
               .SetOperandAndAdvance("Nitrox_IntroUWEPresents")
               // Run our cleanup code when cinematic ends
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldloc_1),
                   new CodeMatch(OpCodes.Ldc_I4_0),
                   new CodeMatch(OpCodes.Call, Reflect.Method((uGUI_SceneIntro si) => si.Stop(default)))
               )
               .Advance(1)
               .Insert(
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => EndRemoteCinematic()))
               )
               .InstructionEnumeration();
    }

    public static bool IsWaitingForPartner { get; private set; }

    private static RemotePlayer partner;
    private static bool callbackRun;
    private static bool packetSend;

    private static bool AnyKeyDownOrModeCompleted()
    {
        return GameInput.AnyKeyDown() || Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.COMPLETED;
    }

    private static float GetSkipTime()
    {
        // Return Time.time when starting solo and disable skip button when staring duo
        return Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.SINGLEPLAYER ? Time.time : -1f;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool IsRemoteCinematicReady(uGUI_SceneIntro uGuiSceneIntro)
    {
        if (callbackRun) return true;
        if (GameModeUtils.currentGameMode.HasFlag(GameModeOption.Creative)) uGuiSceneIntro.Stop(true); // Stopping intro if Creative like in normal SN

        if (Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.COMPLETED)
        {
            uGuiSceneIntro.Stop(true);
            EndRemoteCinematic();
        }

        if (!uGuiSceneIntro.moveNext) return false;

        if (!packetSend)
        {
            uGuiSceneIntro.skipHintStartTime = Time.time;
            uGuiSceneIntro.mainText.SetText(Language.main.GetFormat("Nitrox_IntroWaitingPartner", uGUI.FormatButton(GameInput.Button.UIMenu)));
            uGuiSceneIntro.mainText.SetState(true);

            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.WAITING);
            packetSend = true;
            IsWaitingForPartner = true;
            return false;
        }

        // See NitroxServer.Communication.Packets.Processors.SetIntroCinematicModeProcessor
        RemotePlayer firstWaitingRemotePlayer = Resolve<PlayerManager>().GetAll().FirstOrDefault(r => r.PlayerContext.IntroCinematicMode is IntroCinematicMode.START);
        if (firstWaitingRemotePlayer != null)
        {
            partner = firstWaitingRemotePlayer;
            EnqueueStartCinematic(uGuiSceneIntro);
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.PLAYING);
        }

        return false;
    }

    public static void EnqueueStartCinematic(uGUI_SceneIntro uGuiSceneIntro)
    {
        IsWaitingForPartner = false;
        uGuiSceneIntro.skipHintStartTime = -1;
        uGuiSceneIntro.moveNext = false;
        uGuiSceneIntro.mainText.FadeOut(0.2f, uGuiSceneIntro.Callback);
        callbackRun = true;
    }

    private static void StartRemoteCinematic()
    {
        if (partner != null) // Is null when RunCinematicSingleplayer() is called
        {
            partner.PlayerModel.transform.localScale = new Vector3(-1, 1, 1);
            partner.ArmsController.enabled = false;
            partner.AnimationController.UpdatePlayerAnimations = false;
            partner.AnimationController["cinematics_enabled"] = true;
            partner.AnimationController["escapepod_intro"] = true;

            IntroCinematicUpdater.Partner = partner;
            partner.Body.AddComponent<IntroCinematicUpdater>();
        }
    }

    private static void EndRemoteCinematic()
    {
        if (partner != null)
        {
            IntroCinematicUpdater introCinematicUpdater = partner.Body.GetComponent<IntroCinematicUpdater>();
            if (introCinematicUpdater)
            {
                Object.DestroyImmediate(introCinematicUpdater);
                IntroCinematicUpdater.Partner = null;
            }

            partner.PlayerModel.transform.localScale = new Vector3(1, 1, 1);
            partner.AnimationController["cinematics_enabled"] = false;
            partner.AnimationController["escapepod_intro"] = false;
            partner.ArmsController.enabled = true;
            partner.AnimationController.UpdatePlayerAnimations = true;
        }

        Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
    }
}
