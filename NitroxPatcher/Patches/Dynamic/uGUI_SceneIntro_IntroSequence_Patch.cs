using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.Helpers;
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
               // Replace LargeWorldStreamer.Main.IsWorldSettled() check with IsWorldSettledAndInitialSyncCompleted()
               .MatchStartForward(
                   new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => LargeWorldStreamer.main)),
                   new CodeMatch(OpCodes.Callvirt, Reflect.Method((LargeWorldStreamer lws) => lws.IsWorldSettled()))
               )
               .SetAndAdvance(OpCodes.Call, Reflect.Method(() => IsWorldSettledAndInitialSyncCompleted()))
               .RemoveInstruction()
               // Replace GameInput.AnyKeyDown() check with AnyKeyDownOrSubstitute()
               .MatchEndForward(
                   new CodeMatch(OpCodes.Call, Reflect.Method(() => GameInput.AnyKeyDown()))
               )
               .SetOperandAndAdvance(Reflect.Method(() => AnyKeyDownOrSubstitute()))
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

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool IsWorldSettledAndInitialSyncCompleted()
    {
        return LargeWorldStreamer.main.IsWorldSettled() && Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool AnyKeyDownOrSubstitute()
    {
        return GameInput.AnyKeyDown() ||
               !NitroxEnvironment.IsReleaseMode ||
               Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.COMPLETED;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static float GetSkipTime()
    {
        // Return Time.time when starting solo and disable skip button when staring duo
        return Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.SINGLEPLAYER ? Time.time : -1f;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool IsRemoteCinematicReady(uGUI_SceneIntro uGuiSceneIntro)
    {
        if (callbackRun)
        {
            return true;
        }

        if (Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.COMPLETED)
        {
            SkipLocalCinematic(uGuiSceneIntro, false);
            return false;
        }

        // Skipping intro if creative like in normal SN or in debug configuration
        if (!NitroxEnvironment.IsReleaseMode ||
            GameModeUtils.currentGameMode.HasFlag(GameModeOption.Creative))
        {
            SkipLocalCinematic(uGuiSceneIntro, true);
            return false;
        }

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

        ushort? opPartnerId = Resolve<PlayerCinematics>().IntroCinematicPartnerId;

        if (Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.START &&
            opPartnerId.HasValue && Resolve<PlayerManager>().TryFind(opPartnerId.Value, out RemotePlayer newPartner))
        {
            partner = newPartner;
            EnqueueStartCinematic(uGuiSceneIntro);
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.PLAYING);
            Resolve<PlayerCinematics>().IntroCinematicPartnerId = null;
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
        if (IsPartnerValid()) // Is null when RunCinematicSingleplayer() is called
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
        if (IsPartnerValid())
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

    private static bool IsPartnerValid() => partner != null && Resolve<PlayerManager>().Find(partner.PlayerId).HasValue;

    public static void SkipLocalCinematic(uGUI_SceneIntro uGuiSceneIntro, bool wasNewPlayer)
    {
        LiveMixin radioLiveMixin = EscapePod.main.radioSpawner.spawnedObj.GetComponent<Radio>().liveMixin;
        float radioHealthBefore = radioLiveMixin.health;

        uGuiSceneIntro.Stop(true);

        if (!wasNewPlayer)
        {
            // EscapePod.DamageRadio() is called by GuiSceneIntro.Stop(true) but is undesired. We revert it here
            radioLiveMixin.health = radioHealthBefore;
        }

        if (radioLiveMixin.IsFullHealth())
        {
            Object.Destroy(radioLiveMixin.loopingDamageEffectObj);
        }

        Transform introFireHolder = EscapePod.main.transform.Find("Intro");
        if (introFireHolder) // Can be null if called very early
        {
            introFireHolder.GetComponentInChildren<FMOD_CustomEmitter>(true).ReleaseEvent(); // Not releasing it before destroying results in infinite unstoppable pain
            Object.DestroyImmediate(introFireHolder.gameObject); // Like in Fire.Extinguished() but without delay
        }

        // From uGUI_SceneIntro.IntroSequence() after "if (XRSettings.enabled && VROptions.skipIntro)"
        if (UnityObjectExtensions.TryFind("fire_extinguisher_01_tp", out GameObject gameObject1))
        {
            Object.Destroy(gameObject1);
        }
        if (UnityObjectExtensions.TryFind("IntroFireExtinugisherPickup", out GameObject gameObject2))
        {
            Object.Destroy(gameObject2);
        }

        Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
    }
}
