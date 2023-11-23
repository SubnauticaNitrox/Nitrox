using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementController : MonoBehaviour
{
    public const float LOCATION_BROADCAST_TIME = 0.04f;

    public float Scalar { get; set; } = 1f;
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }
    public bool Receiving { get; private set; }
    public bool Broadcasting { get; private set; }
    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
    }

    public event Action BeforeUpdate = () => {};
    public event Action BeforeFixedUpdate = () => {};
    public event Action AfterUpdate = () => {};
    public event Action AfterFixedUpdate = () => {};

    private Vector3 velocity;
    private float curTime;
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

    private static void StartedSimulatingEntity(NitroxId id)
    {
        if (NitroxEntity.TryGetObjectFrom(id, out GameObject gameObject))
        {
            if (gameObject.TryGetComponent(out MovementController mc))
            {
                mc.SetBroadcasting(true);
            }
        }
    }

    private static void StoppedSimulatingEntity(NitroxId id)
    {
        if (NitroxEntity.TryGetObjectFrom(id, out GameObject gameObject))
        {
            if (gameObject.TryGetComponent(out MovementController mc))
            {
                mc.SetReceiving(true);
            }
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
            transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref velocity, Scalar * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Scalar * Time.deltaTime);
        }

        AfterUpdate();
    }

    private void FixedUpdate()
    {
        BeforeFixedUpdate();

        if (Broadcasting && simulationOwnership.HasAnyLockType(entity.Id))
        {
            curTime += Time.fixedDeltaTime;
            if (curTime >= LOCATION_BROADCAST_TIME)
            {
                curTime = 0f;

                packetSender.Send(new BasicMovement(entity.Id, transform.position.ToDto(), transform.rotation.ToDto()));
            }
        }

        if (rigidbody && Receiving)
        {
            float timing = Scalar * Time.fixedDeltaTime;
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
        simulationOwnership.StoppedSimulatingEntity -= StoppedSimulatingEntity;
    }

    private void OnDisable()
    {
        simulationOwnership.StartedSimulatingEntity -= StartedSimulatingEntity;
        simulationOwnership.StoppedSimulatingEntity -= StoppedSimulatingEntity;
    }
}
