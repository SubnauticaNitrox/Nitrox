using System;
using System.Collections.Generic;
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
    private static readonly List<CharacterController> controllers = [];
    public static readonly int PLAYER_LAYER = 1 << LayerMask.NameToLayer("Player");

    private readonly INitroxPlayer player;
    private readonly NitroxCyclops cyclops;
    private readonly Transform virtualTransform;
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

    public CyclopsPawn(INitroxPlayer player, NitroxCyclops cyclops)
    {
        this.player = player;
        this.cyclops = cyclops;
        virtualTransform = VirtualCyclops.Instance.transform;
        realCyclopsTransform = cyclops.transform;

        if (player is ILocalNitroxPlayer)
        {
            isLocalPlayer = true;
            RealObject = Player.mainObject;
            CyclopsMotor cyclopsMotor = Player.mainObject.GetComponent<CyclopsMotor>();
            MaintainPredicate = () => cyclopsMotor.canControl && !Player.main.isPiloting;
        }
        else if (player is RemotePlayer remotePlayer)
        {
            RealObject = remotePlayer.Body;
            MaintainPredicate = () => !remotePlayer.PilotingChair;
        }

        Initialize($"{player.PlayerName}-Pawn", RealObject.transform.localPosition);
    }

    public void Initialize(string name, Vector3 localPosition)
    {
        PlayerController playerController = Player.main.GetComponent<PlayerController>();
        GroundMotor groundMotor = Player.main.GetComponent<GroundMotor>();
        CharacterController reference = Player.main.GetComponent<CharacterController>();

        Handle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Handle.layer = 1 << PLAYER_LAYER;
        Handle.name = name;
        Handle.transform.parent = virtualTransform;
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
        Controller.stepOffset = groundMotor.controller.stepOffset;
        Controller.slopeLimit = groundMotor.controller.slopeLimit;

        RegisterController();

        Handle.AddComponent<CyclopsPawnIdentifier>().Pawn = this;
    }

    public void RegisterController()
    {
        foreach (CharacterController controller in controllers)
        {
            Physics.IgnoreCollision(controller, Controller);
        }
        controllers.Add(Controller);
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

    public void Unregister()
    {
        if (cyclops)
        {
            if (isLocalPlayer)
            {
                cyclops.OnLocalPlayerExit();
            }
            else
            {
                cyclops.OnPlayerExit((RemotePlayer)player);
            }
        }
    }

    /// <summary>
    /// Replicates openable being blocked only if the pawn causing the block is in the cyclops associated to the virtual one.
    /// </summary>
    public void BlockOpenable(Openable openable, bool blockState)
    {
        if (cyclops.Virtual)
        {
            openable.blocked = blockState;
            cyclops.Virtual.ReplicateBlock(openable, blockState);
        }
    }

    public void Terminate()
    {
        controllers.Remove(Controller);
        GameObject.Destroy(Handle);
    }
}

public class CyclopsPawnIdentifier : MonoBehaviour
{
    public CyclopsPawn Pawn;
}
