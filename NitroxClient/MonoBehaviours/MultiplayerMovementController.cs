using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MultiplayerMovementController : MonoBehaviour
{
    public const float LOCATION_BROADCAST_TIME = 0.04f;

    private static readonly Dictionary<NitroxId, MultiplayerMovementController> movementControllersById = new();

    public float TimeScalar { get; set; } = 1f;
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }
    public bool Receiving { get; private set; }
    public bool Broadcasting { get; private set; }
    public Vector3 Velocity { get; private set; }

    public event Action BeforeUpdate = () => { };
    public event Action BeforeFixedUpdate = () => { };
    public event Action AfterUpdate = () => { };
    public event Action AfterFixedUpdate = () => { };

    private float timeSinceLastBroadcast;
    private Rigidbody rigidbody;
    private IPacketSender packetSender;
    private SimulationOwnership simulationOwnership;
    private NitroxEntity entity;

    public void SetBroadcasting(bool broadcasting)
    {
        Broadcasting = broadcasting;
        if (Receiving && broadcasting)
        {
            Receiving = !broadcasting;
        }
    }

    public void SetReceiving(bool receiving)
    {
        Receiving = receiving;
        if (Broadcasting && receiving)
        {
            Broadcasting = !receiving;
        }
    }

    public static bool TryGetMovementControllerFrom(NitroxId id, out MultiplayerMovementController mc)
    {
        mc = null;
        if (id == null) // Early Exit
        {
            return false;
        }


        if (!movementControllersById.TryGetValue(id, out mc))
        {
            if (!NitroxEntity.TryGetObjectFrom(id, out GameObject gameObject))
            {
                return false;
            }

            if (gameObject.TryGetComponent(out mc) && mc)
            {
                movementControllersById.Add(id, mc);
                return true;
            }
        }

        return mc;
    }

    private static void StartedSimulatingEntity(NitroxId id)
    {
        if (NitroxEntity.TryGetComponentFrom(id, out MultiplayerMovementController mc))
        {
            mc.SetBroadcasting(true);
        }
    }

    private static void StoppedSimulatingEntity(NitroxId id)
    {
        if (NitroxEntity.TryGetComponentFrom(id, out MultiplayerMovementController mc))
        {
            mc.SetReceiving(true);
        }
    }

    private void Awake()
    {
        packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (!TryGetComponent(out NitroxEntity nitroxEntity))
        {
            NitroxEntity.GenerateNewId(gameObject);

            nitroxEntity = GetComponent<NitroxEntity>();
        }
        entity = nitroxEntity;
    }

    private void Update()
    {
        BeforeUpdate();

        if (!rigidbody && Receiving)
        {
            Vector3 velocity = Velocity;
            transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref velocity, TimeScalar * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, TimeScalar * Time.deltaTime);
        }

        AfterUpdate();
    }

    private void FixedUpdate()
    {
        BeforeFixedUpdate();

        if (Broadcasting && simulationOwnership.HasAnyLockType(entity.Id))
        {
            timeSinceLastBroadcast += Time.fixedDeltaTime;
            if (timeSinceLastBroadcast >= LOCATION_BROADCAST_TIME)
            {
                timeSinceLastBroadcast = 0f;

                packetSender.Send(new BasicMovement(entity.Id, transform.position.ToDto(), transform.rotation.ToDto()));
            }
        }

        if (rigidbody && Receiving)
        {
            Vector3 velocity = Velocity;
            float timing = TimeScalar * Time.fixedDeltaTime;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, TargetPosition, ref velocity, timing);

            if (rigidbody.isKinematic)
            {
                rigidbody.MovePosition(newPos);
                rigidbody.MoveRotation(TargetRotation);
            }
            else
            {
                rigidbody.velocity = velocity;
                rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(TargetRotation, gameObject, timing);
            }
        }

        AfterFixedUpdate();
    }

    private void OnEnable()
    {
        simulationOwnership.StartedSimulatingEntity += StartedSimulatingEntity;
        simulationOwnership.StoppedSimulatingEntity += StoppedSimulatingEntity;
    }

    private void OnDisable()
    {
        simulationOwnership.StartedSimulatingEntity -= StartedSimulatingEntity;
        simulationOwnership.StoppedSimulatingEntity -= StoppedSimulatingEntity;
    }
}
