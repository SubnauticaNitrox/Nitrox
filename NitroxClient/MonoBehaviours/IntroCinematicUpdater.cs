// TODO: Fix intro
using System.Collections;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class IntroCinematicUpdater : MonoBehaviour
{
    public static RemotePlayer Partner;
    private static Transform modelRoot;

    private static readonly Transform playerTransform = Player.main.transform;
    private static readonly Quaternion rotateOtherSide = Quaternion.Euler(0, 180, 0);

    private Transform introEndTarget;
    private Vector3 introRemoteEndPosition;
    private Transform[] seatPartsLeft;
    private Transform[] seatPartsRight;
    private Transform seatArmRestLeft;
    private Transform seatArmRestRight;
    private SkinnedMeshRenderer[] seatBarRendererRight = [];

    private SkinnedMeshRenderer remotePlayerHeadRenderer;
    private GameObject remotePlayerCustomHead;
    private SkinnedMeshRenderer[] remoteRenders = [];

    public void Awake()
    {
        //if (Partner == null)
        //{
        //    Log.Error($"[{nameof(IntroCinematicUpdater)}.Awake()]: Partner was null. Disabling MonoBehaviour.");
        //    enabled = false;
        //    return;
        //}
        //modelRoot = EscapePod.main.transform.Find("models/Life_Pod_damaged_03/root");
        //Transform seatLeft = modelRoot.Find("life_pod_seat_01_left_damaged_jnt1");
        //Transform seatRight = modelRoot.Find("life_pod_seat_01_right_damaged_jnt1");

        //seatPartsLeft = new[]
        //{
        //    seatLeft.Find("life_pod_seat_01_left_damaged_jnt2"),
        //    seatLeft.Find("life_pod_seat_01_left_damaged_jnt3")
        //};

        //seatPartsRight = new[]
        //{
        //    seatRight.Find("life_pod_seat_01_right_damaged_jnt2"),
        //    seatRight.Find("life_pod_seat_01_right_damaged_jnt3")
        //};

        //seatArmRestLeft = modelRoot.Find("life_pod_seat_01_left_damaged_jnt4/life_pod_seat_01_left_damaged_jnt5");
        //seatArmRestRight = modelRoot.Find("life_pod_seat_01_right_damaged_jnt4/life_pod_seat_01_right_damaged_jnt5");

        //seatBarRendererRight = modelRoot.parent.Find("lifepod_damaged_03_geo/life_pod_seat_01_R").GetComponentsInChildren<SkinnedMeshRenderer>();

        //remoteRenders = Partner.PlayerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        //remotePlayerHeadRenderer = Partner.PlayerModel.transform.Find("male_geo/diveSuit/diveSuit_head_geo").GetComponent<SkinnedMeshRenderer>();

        //remotePlayerCustomHead = Instantiate(new GameObject("diveSuit_custom_head_geo"), Partner.PlayerModel.transform.Find("export_skeleton/head_rig"));
        //remotePlayerCustomHead.transform.localPosition = new Vector3(1.0941f, -0.1163f, -1.2107f);
        //remotePlayerCustomHead.transform.localRotation = new Quaternion(-0.05750692f, -0.5272675f, -0.6740523f, -0.5141357f);

        //MeshFilter mesh = remotePlayerCustomHead.AddComponent<MeshFilter>();
        //MeshRenderer meshRenderer = remotePlayerCustomHead.AddComponent<MeshRenderer>();
        //mesh.mesh = remotePlayerHeadRenderer.sharedMesh;
        //meshRenderer.materials = remotePlayerHeadRenderer.materials;
        //remotePlayerCustomHead.SetActive(false);

        //bool isLeftPlayer = NitroxServiceLocator.Cache<LocalPlayer>.Value.PlayerId < Partner.PlayerId;
        //introEndTarget = EscapePod.main.transform.Find("EscapePodCinematics/intro_end");
        //Vector3 introEndDiff = isLeftPlayer ? new Vector3(0, 0, 0.3f) : new Vector3(0, 0, -0.3f);
        //introRemoteEndPosition = introEndTarget.position - introEndDiff;
        //introEndTarget.position += introEndDiff;
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
        foreach (SkinnedMeshRenderer renderer in remoteRenders)
        {
            if (renderer)
            {
                renderer.enabled = true;
            }
        }

        foreach (SkinnedMeshRenderer renderer in seatBarRendererRight)
        {
            if (renderer)
            {
                renderer.updateWhenOffscreen = false;
            }
        }

        transform.position = introRemoteEndPosition;
    }
}
