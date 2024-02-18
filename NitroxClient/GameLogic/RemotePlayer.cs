using System.Collections.Generic;
using System.Collections.ObjectModel;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.MultiplayerSession;
using NitroxModel.Server;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic;

public class RemotePlayer : INitroxPlayer
{
    private static readonly int animatorPlayerIn = Animator.StringToHash("player_in");

    private readonly PlayerModelManager playerModelManager;
    private readonly PlayerVitalsManager playerVitalsManager;
    private readonly FMODWhitelist fmodWhitelist;

    public PlayerContext PlayerContext { get; }
    public GameObject Body { get; private set; }
    public GameObject PlayerModel { get; private set; }
    public Rigidbody RigidBody { get; private set; }
    public CapsuleCollider Collider { get; private set; }
    public ArmsController ArmsController { get; private set; }
    public AnimationController AnimationController { get; private set; }
    public ItemsContainer Inventory { get; private set; }
    public Transform ItemAttachPoint { get; private set; }
    public RemotePlayerVitals vitals { get; private set; }

    public ushort PlayerId => PlayerContext.PlayerId;
    public string PlayerName => PlayerContext.PlayerName;
    public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;

    public Vehicle Vehicle { get; private set; }
    public SubRoot SubRoot { get; private set; }
    public EscapePod EscapePod { get; private set; }
    public PilotingChair PilotingChair { get; private set; }

    public readonly Event<RemotePlayer> PlayerDeathEvent = new();

    public readonly Event<RemotePlayer> PlayerDisconnectEvent = new();

    public RemotePlayer(PlayerContext playerContext, PlayerModelManager playerModelManager, PlayerVitalsManager playerVitalsManager, FMODWhitelist fmodWhitelist)
    {
        PlayerContext = playerContext;
        this.playerModelManager = playerModelManager;
        this.playerVitalsManager = playerVitalsManager;
        this.fmodWhitelist = fmodWhitelist;
    }

    public void InitializeGameObject(GameObject playerBody)
    {
        Body = playerBody;
        Body.name = PlayerName;

        RigidBody = Body.AddComponent<Rigidbody>();
        RigidBody.useGravity = false;
        RigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        NitroxEntity.SetNewId(Body, PlayerContext.PlayerNitroxId);

        // Get player
        PlayerModel = Body.RequireGameObject("player_view");
        // Move variables to keep player animations from mirroring and for identification
        ArmsController = PlayerModel.GetComponent<ArmsController>();
        ArmsController.smoothSpeedUnderWater = 0;
        ArmsController.smoothSpeedAboveWater = 0;

        // ConditionRules has Player.Main based conditions from ArmsController
        PlayerModel.GetComponent<ConditionRules>().enabled = false;

        AnimationController = PlayerModel.AddComponent<AnimationController>();

        Transform inventoryTransform = new GameObject("Inventory").transform;
        inventoryTransform.SetParent(Body.transform);
        Inventory = new ItemsContainer(6, 8, inventoryTransform, $"NitroxInventoryStorage_{PlayerName}", null);

        ItemAttachPoint = PlayerModel.transform.Find(PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME);

        CoroutineUtils.StartCoroutineSmart(playerModelManager.AttachPing(this));
        playerModelManager.BeginApplyPlayerColor(this);
        playerModelManager.RegisterEquipmentVisibilityHandler(PlayerModel);
        SetupBody();
        SetupSkyAppliers();
        SetupPlayerSounds();

        vitals = playerVitalsManager.CreateOrFindForPlayer(this);
        RefreshVitalsVisibility();
    }

    public void Attach(Transform transform, bool keepWorldTransform = false)
    {
        Body.transform.SetParent(transform);

        if (!keepWorldTransform)
        {
            UWE.Utils.ZeroTransform(Body);
        }

        SkyEnvironmentChanged.Broadcast(Body, transform);
    }

    public void Detach()
    {
        Body.transform.SetParent(null);
        SkyEnvironmentChanged.Broadcast(Body, (GameObject)null);
    }

    public void UpdatePosition(Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
    {
        Body.SetActive(true);

        // When receiving movement packets, a player can not be controlling a vehicle (they can walk through subroots though).
        SetVehicle(null);
        SetPilotingChair(null);
        // If in a subroot the position will be relative to the subroot
        if (SubRoot && !SubRoot.isBase)
        {
            Quaternion vehicleAngle = SubRoot.transform.rotation;
            position = vehicleAngle * position;
            position += SubRoot.transform.position;
            bodyRotation = vehicleAngle * bodyRotation;
            aimingRotation = vehicleAngle * aimingRotation;
        }

        RigidBody.velocity = AnimationController.Velocity = MovementHelper.GetCorrectedVelocity(position, velocity, Body, Time.fixedDeltaTime * (PlayerMovementBroadcaster.LOCATION_BROADCAST_TICK_SKIPS + 1));
        RigidBody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(bodyRotation, Vector3.zero, Body, Time.fixedDeltaTime * (PlayerMovementBroadcaster.LOCATION_BROADCAST_TICK_SKIPS + 1));

        AnimationController.AimingRotation = aimingRotation;
        AnimationController.UpdatePlayerAnimations = true;
    }

    public void SetPilotingChair(PilotingChair newPilotingChair)
    {
        if (PilotingChair != newPilotingChair)
        {
            PilotingChair = newPilotingChair;

            MultiplayerCyclops mpCyclops = null;

            // For unexpected and expected cases, for example when a player is driving a cyclops but the cyclops is destroyed
            if (!SubRoot)
            {
                Log.Error("Player changed PilotingChair but is not in SubRoot!");
            }
            else
            {
                mpCyclops = SubRoot.GetComponent<MultiplayerCyclops>();
            }

            if (PilotingChair)
            {
                Attach(PilotingChair.sittingPosition.transform);
                ArmsController.SetWorldIKTarget(PilotingChair.leftHandPlug, PilotingChair.rightHandPlug);

                mpCyclops.CurrentPlayer = this;
                mpCyclops.Enter();
            }
            else
            {
                SetSubRoot(SubRoot);
                ArmsController.SetWorldIKTarget(null, null);

                if (mpCyclops)
                {
                    mpCyclops.CurrentPlayer = null;
                    mpCyclops.Exit();
                }
            }

            RigidBody.isKinematic = AnimationController["cyclops_steering"] = newPilotingChair;
        }
    }

    public void SetSubRoot(SubRoot newSubRoot)
    {
        if (SubRoot != newSubRoot)
        {
            if (newSubRoot)
            {
                Attach(newSubRoot.transform, true);
            }
            else
            {
                Detach();
            }

            SubRoot = newSubRoot;
        }
    }

    public void SetEscapePod(EscapePod newEscapePod)
    {
        if (EscapePod != newEscapePod)
        {
            if (newEscapePod)
            {
                Attach(newEscapePod.transform, true);
            }
            else
            {
                Detach();
            }

            EscapePod = newEscapePod;
        }
    }

    public void SetVehicle(Vehicle newVehicle)
    {
        if (Vehicle != newVehicle)
        {
            if (Vehicle)
            {
                Vehicle.mainAnimator.SetBool(animatorPlayerIn, false);

                Detach();
                ArmsController.SetWorldIKTarget(null, null);

                Vehicle.GetComponent<MultiplayerVehicleControl>().Exit();
            }

            if (newVehicle)
            {
                newVehicle.mainAnimator.SetBool(animatorPlayerIn, true);

                Attach(newVehicle.playerPosition.transform);
                ArmsController.SetWorldIKTarget(newVehicle.leftHandPlug, newVehicle.rightHandPlug);

                // From here, a basic issue can happen.
                // When a vehicle is docked since we joined a game and another player undocks him before the local player does,
                // no MultiplayerVehicleControl can be found on the vehicle because they are only created when receiving VehicleMovement packets
                // Therefore we need to make sure that the MultiplayerVehicleControl component exists before using it
                switch (newVehicle)
                {
                    case SeaMoth:
                        newVehicle.gameObject.EnsureComponent<MultiplayerSeaMoth>().Enter();
                        break;
                    case Exosuit:
                        newVehicle.gameObject.EnsureComponent<MultiplayerExosuit>().Enter();
                        break;
                }
            }

            RigidBody.isKinematic = newVehicle;

            Vehicle = newVehicle;

            AnimationController["in_seamoth"] = newVehicle is SeaMoth;
            AnimationController["in_exosuit"] = AnimationController["using_mechsuit"] = newVehicle is Exosuit;
        }
    }

    /// <summary>
    /// Drops the remote player, swimming where he is
    /// </summary>
    public void ResetStates()
    {
        SetVehicle(null);
        SetSubRoot(null);
        AnimationController.UpdatePlayerAnimations = true;
    }

    public void Destroy()
    {
        Log.Info($"{PlayerName} left the game");
        Log.InGame(Language.main.Get("Nitrox_PlayerLeft").Replace("{PLAYER}", PlayerName));
        NitroxEntity.RemoveFrom(Body);
        Object.DestroyImmediate(Body);
    }

    public void UpdateAnimationAndCollider(AnimChangeType type, AnimChangeState state)
    {
        switch (type)
        {
            case AnimChangeType.UNDERWATER:
                AnimationController["is_underwater"] = state != AnimChangeState.OFF;
                break;
            case AnimChangeType.BENCH:
                AnimationController["cinematics_enabled"] = state != AnimChangeState.UNSET;
                AnimationController["bench_sit"] = state == AnimChangeState.ON;
                AnimationController["bench_stand_up"] = state == AnimChangeState.OFF;
                break;
        }

        // Rough estimation for different collider boxes in different animation stages
        if (AnimationController["is_underwater"])
        {
            Collider.center = new(0f, -0.3f, 0f);
            Collider.height = 0.5f;
        }
        else
        {
            Collider.center = new(0f, -0.8f, 0f);
            Collider.height = 1.5f;
        }
    }

    public void UpdateEquipmentVisibility(List<TechType> equippedItems)
    {
        playerModelManager.UpdateEquipmentVisibility(new ReadOnlyCollection<TechType>(equippedItems));
    }

    /// <summary>
    /// Makes the RemotePlayer recognizable as an obstacle for buildings.
    /// </summary>
    private void SetupBody()
    {
        RemotePlayerIdentifier identifier = Body.AddComponent<RemotePlayerIdentifier>();
        identifier.RemotePlayer = this;

        if (Player.mainCollider is CapsuleCollider refCollider)
        {
            // This layer lets us have a collider as a trigger without preventing its detection as an obstacle
            Body.layer = LayerID.Useable;
            Collider = Body.AddComponent<CapsuleCollider>();

            Collider.center = Vector3.zero;
            Collider.radius = refCollider.radius;
            Collider.direction = refCollider.direction;
            Collider.contactOffset = refCollider.contactOffset;
            Collider.isTrigger = true;
        }
        else
        {
            Log.Warn("The main collider of the main Player couldn't be found or is not a CapsuleCollider. Collisions for the RemotePlayer won't be created");
        }
    }

    /// <summary>
    /// Allows the remote player model to have its lighting dynamically adjusted
    /// </summary>
    private void SetupSkyAppliers()
    {
        // SkyAppliers apply the light effects of a lighting source on a set of renderers
        SkyApplier skyApplier = Body.AddComponent<SkyApplier>();
        skyApplier.anchorSky = Skies.Auto;
        skyApplier.emissiveFromPower = false;
        skyApplier.dynamic = true;
        skyApplier.renderers = Body.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    }

    /// <summary>
    /// Sets up all bubbles, breathing and diving sounds to be multiplayer ready
    /// </summary>
    private void SetupPlayerSounds()
    {
        GameObject remotePlayerSoundsRoot = new("RemotePlayerSounds");
        remotePlayerSoundsRoot.transform.SetParent(Body.transform);
        FMODEmitterController emitterController = Body.AddComponent<FMODEmitterController>();

        static void CopyEmitter(FMOD_CustomEmitter src, FMOD_CustomEmitter dst)
        {
            dst.asset = src.asset;
            dst.playOnAwake = src.playOnAwake;
            dst.stopImmediatelyOnDisable = src.stopImmediatelyOnDisable;
            dst.followParent = src.followParent;
            dst.restartOnPlay = src.restartOnPlay;
        }

        // Bubbles
        PlayerBreathBubbles localPlayerBubbles = Player.main.GetComponentInChildren<PlayerBreathBubbles>(true);
        FMOD_CustomEmitter bubblesCustomEmitter = remotePlayerSoundsRoot.AddComponent<FMOD_CustomEmitter>();
        CopyEmitter(localPlayerBubbles.bubbleSound, bubblesCustomEmitter);

        if (fmodWhitelist.IsWhitelisted(bubblesCustomEmitter.asset.path, out float bubblesSoundRadius))
        {
            emitterController.AddEmitter(bubblesCustomEmitter.asset.path, bubblesCustomEmitter, bubblesSoundRadius);
        }
        else
        {
            Log.Error($"[{nameof(RemotePlayer)}] Manual created FMOD emitter for {nameof(PlayerBreathBubbles)} but linked sound is not whitelisted: ({bubblesCustomEmitter.asset.path})");
        }

        // Breathing
        BreathingSound breathingSound = Player.main.GetComponentInChildren<BreathingSound>(true);
        FMOD_CustomEmitter breathingSoundCustomEmitter = remotePlayerSoundsRoot.AddComponent<FMOD_CustomEmitter>();
        breathingSoundCustomEmitter.asset = breathingSound.loopingBreathingSound.asset;

        if (fmodWhitelist.IsWhitelisted(breathingSoundCustomEmitter.asset.path, out float breathingSoundRadius))
        {
            emitterController.AddEmitter(breathingSoundCustomEmitter.asset.path, breathingSoundCustomEmitter, breathingSoundRadius);
        }
        else
        {
            Log.Error($"[{nameof(RemotePlayer)}] Manual created FMOD emitter for {nameof(BreathingSound)} but linked sound is not whitelisted: ({breathingSoundCustomEmitter.asset.path})");
        }

        // Diving
        WaterAmbience waterAmbience = Player.main.GetComponentInChildren<WaterAmbience>(true);
        FMOD_CustomEmitter diveStartCustomEmitter = remotePlayerSoundsRoot.AddComponent<FMOD_CustomEmitter>();
        CopyEmitter(waterAmbience.diveStartSplash, diveStartCustomEmitter);

        if (fmodWhitelist.IsWhitelisted(diveStartCustomEmitter.asset.path, out float diveSoundRadius))
        {
            emitterController.AddEmitter(diveStartCustomEmitter.asset.path, diveStartCustomEmitter, diveSoundRadius);
        }
        else
        {
            Log.Error($"[{nameof(RemotePlayer)}] Manual created FMOD emitter for {nameof(WaterAmbience)} but linked sound is not whitelisted: ({diveStartCustomEmitter.asset.path})");
        }
    }

    public void SetGameMode(NitroxGameMode gameMode)
    {
        PlayerContext.GameMode = gameMode;
        RefreshVitalsVisibility();
    }

    private void RefreshVitalsVisibility()
    {
        if (vitals)
        {
            vitals.gameObject.SetActive(PlayerContext.GameMode != NitroxGameMode.CREATIVE);
        }
    }
}
