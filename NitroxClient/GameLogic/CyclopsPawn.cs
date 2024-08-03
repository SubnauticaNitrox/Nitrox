using System;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Cyclops;
using UnityEngine;

namespace NitroxClient.GameLogic;

/// <summary>
/// A virtual entity responsible for one player's movement in the cyclops.
/// It simulates the local player's movements by creating a fake player in a <see cref="VirtualCyclops"/>'s instance and then giving data about the real movement.
/// </summary>
public class CyclopsPawn
{
    private readonly Transform parentTransform;
    private readonly Transform realCyclopsTransform;
    private readonly bool isLocalPlayer;
    public readonly GameObject RealObject;
    public GameObject Handle;
    public CharacterController Controller;
    public Func<bool> MaintainPredicate;

    public Vector3 Position
    {
        get => Handle.transform.position;
        set { Handle.transform.position = value; }
    }

    public CyclopsPawn(INitroxPlayer player, VirtualCyclops virtualCyclops, Transform realCyclopsTransform)
    {
        parentTransform = virtualCyclops.transform;
        this.realCyclopsTransform = realCyclopsTransform;

        if (player is ILocalNitroxPlayer)
        {
            isLocalPlayer = true;
            RealObject = Player.mainObject;
            CyclopsMotor cyclopsMotor = Player.mainObject.GetComponent<CyclopsMotor>();
            MaintainPredicate = () => cyclopsMotor.canControl && !Player.main.isPiloting;
        }
        else if (player is RemotePlayer remotePlayer)
        {
            RealObject = player.Body;
            MaintainPredicate = () => true;
        }

        Initialize($"{player.PlayerName}-Pawn", RealObject.transform.localPosition);
    }

    public void Initialize(string name, Vector3 localPosition)
    {
        PlayerController playerController = Player.main.GetComponent<PlayerController>();
        GroundMotor groundMotor = Player.main.GetComponent<GroundMotor>();
        CharacterController reference = Player.main.GetComponent<CharacterController>();

        Handle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Handle.layer = LayerMask.NameToLayer("Player");
        Handle.name = name;
        Handle.transform.parent = parentTransform;
        Handle.transform.localPosition = localPosition;
        GameObject.DestroyImmediate(Handle.GetComponent<Collider>());

        Controller = Handle.AddComponent<CharacterController>();
        Controller.height = playerController.standheight - playerController.cameraOffset;
        // Calculation from Groundmotor.SetControllerHeight
        Vector3 center = groundMotor.colliderCenter;
        center.y = -Controller.height * 0.5f - playerController.cameraOffset;
        Controller.center = center;
        Controller.radius = playerController.controllerRadius;
        Controller.skinWidth = reference.skinWidth;
        Controller.stepOffset = groundMotor.controllerSetup.stepOffset;
        Controller.slopeLimit = groundMotor.controllerSetup.slopeLimit;
        Log.Debug($"Pawn: height: {Controller.height}, center {center}, radius: {Controller.radius}, skinWidth: {Controller.skinWidth}");
    }

    public void SetReference()
    {
        Handle.transform.localPosition = RealObject.transform.localPosition;
        if (!isLocalPlayer)
        {
            Handle.transform.localRotation = RealObject.transform.localRotation;
        }
    }

    public void MaintainPosition()
    {
        RealObject.transform.localPosition = Handle.transform.localPosition;
        RealObject.transform.rotation = realCyclopsTransform.rotation;
        if (!isLocalPlayer)
        {
            RealObject.transform.localRotation = Handle.transform.localRotation;
        }
    }

    public void Terminate()
    {
        GameObject.Destroy(Handle);
    }
}
