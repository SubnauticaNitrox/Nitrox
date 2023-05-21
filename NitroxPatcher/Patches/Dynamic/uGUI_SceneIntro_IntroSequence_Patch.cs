using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_SceneIntro_IntroSequence_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = AccessTools.EnumeratorMoveNext(Reflect.Method((uGUI_SceneIntro si) => si.IntroSequence()));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldfld, Reflect.Field((uGUI_SceneIntro si) => si.moveNext)),
                   new CodeMatch(OpCodes.Brfalse))
               .Insert(
                   new CodeInstruction(OpCodes.Ldloc_1),
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => IsRemoteCinematicReady(default))),
                   new CodeInstruction(OpCodes.And)
               )
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => EscapePod.main)),
                   new CodeMatch(OpCodes.Callvirt, Reflect.Method(() => EscapePod.main.TriggerIntroCinematic()))
               )
               .Advance(1)
               .Insert(
                   new CodeInstruction(OpCodes.Call, Reflect.Method(() => StartRemoteCinematic()))
               )
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldstr, "IntroUWEPresents")
               )
               .SetOperandAndAdvance("Nitrox_IntroUWEPresents")
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

    private static RemotePlayer partner;
    private static bool callbackRun;
    private static bool packetSend;

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool IsRemoteCinematicReady(uGUI_SceneIntro uGuiSceneIntro)
    {
        if (callbackRun) return true;
        if (!uGuiSceneIntro.moveNext) return false;

        if (!packetSend)
        {
            uGuiSceneIntro.mainText.SetText("Waiting for partner to join");
            uGuiSceneIntro.mainText.SetState(true);

            ushort playerId = Resolve<LocalPlayer>().PlayerId;
            Resolve<IPacketSender>().Send(new SetIntroCinematicMode(playerId, SetIntroCinematicMode.IntroCinematicMode.WAITING));
            packetSend = true;
            return false;
        }

        if (Resolve<PlayerManager>().GetAllRemotePlayers().Any(r => r.PlayerContext.IntroCinematicMode == SetIntroCinematicMode.IntroCinematicMode.START))
        {
            partner = Resolve<PlayerManager>().GetAllRemotePlayers().First(r => r.PlayerContext.IntroCinematicMode == SetIntroCinematicMode.IntroCinematicMode.START);

            uGuiSceneIntro.moveNext = false;
            uGuiSceneIntro.mainText.FadeOut(0.2f, uGuiSceneIntro.Callback);
            callbackRun = true;
        }

        return false;
    }

    private static void StartRemoteCinematic()
    {
        Validate.NotNull(partner);

        partner.PlayerModel.transform.localScale = new Vector3(-1, 1, 1); //-1
        partner.ArmsController.enabled = false;
        partner.AnimationController.UpdatePlayerAnimations = false;
        partner.AnimationController["cinematics_enabled"] = true;
        partner.AnimationController["escapepod_intro"] = true;

        partner.Body.AddComponent<IntroCinematicUpdater>();
    }

    public class IntroCinematicUpdater : MonoBehaviour
    {
        private static readonly Transform playerTransform = Player.main.transform;
        private static readonly Quaternion rotateOtherSide = Quaternion.Euler(0, 180, 0);
        private static Transform modelRoot;

        private Transform introEndTarget;
        private Vector3 introRemoteEndPosition;
        private Transform[] seatPartsLeft;
        private Transform[] seatPartsRight;
        private Transform seatArmRestLeft;
        private Transform seatArmRestRight;
        private SkinnedMeshRenderer[] seatBarRendererRight;

        private SkinnedMeshRenderer remotePlayerHeadRenderer;
        private GameObject remotePlayerCustomHead;
        private SkinnedMeshRenderer[] remoteRenders;

        public void Awake()
        {
            modelRoot = EscapePod.main.transform.Find("models/Life_Pod_damaged_03/root");
            Transform seatLeft = modelRoot.Find("life_pod_seat_01_left_damaged_jnt1");
            Transform seatRight = modelRoot.Find("life_pod_seat_01_right_damaged_jnt1");

            seatPartsLeft = new[]
            {
                seatLeft.Find("life_pod_seat_01_left_damaged_jnt2"),
                seatLeft.Find("life_pod_seat_01_left_damaged_jnt3")
            };

            seatPartsRight = new[]
            {
                seatRight.Find("life_pod_seat_01_right_damaged_jnt2"),
                seatRight.Find("life_pod_seat_01_right_damaged_jnt3")
            };

            seatArmRestLeft =  modelRoot.Find("life_pod_seat_01_left_damaged_jnt4/life_pod_seat_01_left_damaged_jnt5");
            seatArmRestRight = modelRoot.Find("life_pod_seat_01_right_damaged_jnt4/life_pod_seat_01_right_damaged_jnt5");

            seatBarRendererRight = modelRoot.parent.Find("lifepod_damaged_03_geo/life_pod_seat_01_R").GetComponentsInChildren<SkinnedMeshRenderer>();

            remoteRenders = partner.PlayerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            remotePlayerHeadRenderer = partner.PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_head_geo").GetComponent<SkinnedMeshRenderer>();

            remotePlayerCustomHead = Instantiate(new GameObject("diveSuit_custom_head_geo"), partner.PlayerModel.transform.Find("export_skeleton/head_rig"));
            remotePlayerCustomHead.transform.localPosition = new Vector3(1.0941f, -0.1163f, -1.2107f);
            remotePlayerCustomHead.transform.localRotation = new Quaternion(-0.05750692f, -0.5272675f, -0.6740523f, -0.5141357f);

            MeshFilter mesh = remotePlayerCustomHead.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = remotePlayerCustomHead.AddComponent<MeshRenderer>();
            mesh.mesh = remotePlayerHeadRenderer.sharedMesh;
            meshRenderer.materials = remotePlayerHeadRenderer.materials;
            remotePlayerCustomHead.SetActive(false);

            bool isLeftPlayer = Resolve<LocalPlayer>().PlayerId < partner.PlayerId;
            introEndTarget = EscapePod.main.transform.Find("EscapePodCinematics/intro_end");
            Vector3 introEndDiff = isLeftPlayer ? new Vector3(0, 0, 0.3f) : new Vector3(0, 0, -0.3f);
            introRemoteEndPosition = introEndTarget.position - introEndDiff;
            introEndTarget.position += introEndDiff;
        }

        private Vector3 remotePlayerDestinationPos;
        private Quaternion remotePlayerDestinationRot;

        private bool shouldDisplaceLocalPlayer;
        private float localPlayerDisplacementTime;

        private bool shouldDisplaceRemotePlayer;
        private Vector3 remotePlayerDisplacement = Vector3.zero;

        private IEnumerator Start()
        {
            remoteRenders.ForEach(smr => smr.enabled = false);
            seatBarRendererRight.ForEach(smr => smr.updateWhenOffscreen = true);

            yield return new WaitForSecondsRealtime(11f); //Local player in seat
            remoteRenders.ForEach(smr => smr.enabled = true);

            yield return new WaitForSecondsRealtime(33.45f - 11f); //Remote Player deadly hit => disable original head
            remotePlayerHeadRenderer.enabled = false;

            yield return new WaitForSecondsRealtime(33.58f - 33.45f); //Remote Player visible => enable custom head
            remotePlayerCustomHead.SetActive(true);

            yield return new WaitForSecondsRealtime(40f - 33.58f); //Seatbelt appears => enable position sync & switch to original head
            remotePlayerCustomHead.SetActive(false);
            remotePlayerHeadRenderer.enabled = true;
            Destroy(remotePlayerCustomHead);

            yield return new WaitForSecondsRealtime(50.5f - 40f); //Slowly move remote player backwards
            shouldDisplaceRemotePlayer = true;

            yield return new WaitForSecondsRealtime(50.72f - 50.5f); //Disable and move remote player
            shouldDisplaceRemotePlayer = false;
            remotePlayerDisplacement = Vector3.zero;
            remoteRenders.ForEach(smr => smr.enabled = false);

            yield return new WaitForSecondsRealtime(53f - 50.72f); //Slowly move local player to modified cin-end
            shouldDisplaceLocalPlayer = true;
        }

        private void LateUpdate()
        {
            Vector3 modelRootPos = modelRoot.position;
            Quaternion modelRootRot = modelRoot.rotation;

            // Coping the global rotation, inverting the y and adding 180° to it
            Quaternion InverseRotateAroundEscapePod(Quaternion from)
            {
                Quaternion offset = Quaternion.Inverse(modelRootRot) * from;
                return modelRootRot * rotateOtherSide * new Quaternion(-offset.x, offset.y, offset.z, -offset.w);
            }

            // Mirror animate the remote like the local player
            Vector3 posDiff = playerTransform.position - modelRootPos;
            remotePlayerDestinationPos = modelRootPos + new Vector3(posDiff.x, posDiff.y, -posDiff.z) + remotePlayerDisplacement;
            remotePlayerDestinationRot = InverseRotateAroundEscapePod(playerTransform.rotation);
            transform.SetPositionAndRotation(remotePlayerDestinationPos, remotePlayerDestinationRot);

            // Mirror animate the seat parts and stop it before the animation ends so the SN animator resets our manipulations
            if (!shouldDisplaceLocalPlayer)
            {
                for (int i = 0; i < seatPartsLeft.Length; i++)
                {
                    seatPartsRight[i].localRotation = seatPartsLeft[i].localRotation;
                }
                seatArmRestRight.localPosition = -seatArmRestLeft.localPosition;
            }

            if (shouldDisplaceRemotePlayer) remotePlayerDisplacement += new Vector3(0, 0, 0.05f);

            if (shouldDisplaceLocalPlayer)
            {
                Vector3 introEndTargetPos = introEndTarget.position;
                playerTransform.position = Vector3.Lerp(playerTransform.position, introEndTargetPos, localPlayerDisplacementTime);
                MainCameraControl.main.transform.position = Vector3.Lerp(MainCameraControl.main.transform.position, introEndTargetPos, localPlayerDisplacementTime);

                localPlayerDisplacementTime += 0.01f;
            }
        }

        private void OnDestroy()
        {
            remoteRenders.ForEach(smr => smr.enabled = true);
            seatBarRendererRight.ForEach(smr => smr.updateWhenOffscreen = false);
            transform.position = introRemoteEndPosition;
        }
    }

    private static void EndRemoteCinematic()
    {
        Object.DestroyImmediate(partner.Body.GetComponent<IntroCinematicUpdater>());
        partner.PlayerModel.transform.localScale = new Vector3(1, 1, 1);
        partner.AnimationController["cinematics_enabled"] = false;
        partner.AnimationController["escapepod_intro"] = false;
        partner.ArmsController.enabled = true;
        partner.AnimationController.UpdatePlayerAnimations = true;

        ushort playerId = Resolve<LocalPlayer>().PlayerId;
        Resolve<IPacketSender>().Send(new SetIntroCinematicMode(playerId, SetIntroCinematicMode.IntroCinematicMode.COMPLETED));
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, targetMethod);
    }
}
