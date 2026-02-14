using System;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic;

public class LocalPlayer : ILocalNitroxPlayer
{
    private readonly IMultiplayerSession multiplayerSession;
    private readonly IPacketSender packetSender;
    private readonly ThrottledPacketSender throttledPacketSender;
    private readonly Lazy<GameObject> body;
    private readonly Lazy<GameObject> playerModel;
    private readonly Lazy<GameObject> bodyPrototype;

    public GameObject Body => body.Value;

    public GameObject PlayerModel => playerModel.Value;

    public GameObject BodyPrototype => bodyPrototype.Value;

    public string PlayerName => multiplayerSession.AuthenticationContext.Username;
    /// <summary>
    ///     Gets the player id. The session is lost on disconnect so this can return null.
    /// </summary>
    public SessionId? SessionId => multiplayerSession.Reservation?.SessionId;
    public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

    public Perms Permissions { get; set; }
    public IntroCinematicMode IntroCinematicMode { get; set; }
    public bool KeepInventoryOnDeath { get; set; }

    public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender, ThrottledPacketSender throttledPacketSender)
    {
        this.multiplayerSession = multiplayerSession;
        this.packetSender = packetSender;
        this.throttledPacketSender = throttledPacketSender;
        body = new Lazy<GameObject>(() => Player.main.RequireGameObject("body"));
        playerModel = new Lazy<GameObject>(() => Body.RequireGameObject("player_view"));
        bodyPrototype = new Lazy<GameObject>(CreateBodyPrototype);
        Permissions = Perms.PLAYER;
        IntroCinematicMode = IntroCinematicMode.NONE;
        KeepInventoryOnDeath = false;
    }

    public void BroadcastLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
    {
        if (!SessionId.HasValue)
        {
            return;
        }

        PlayerMovement playerMovement = new(SessionId.Value, location.ToDto(), velocity.ToDto(), bodyRotation.ToDto(), aimingRotation.ToDto());

        packetSender.Send(playerMovement);
    }

    public void AnimationChange(AnimChangeType type, AnimChangeState state)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new AnimationChangeEvent(SessionId.Value, new(type, state)));
        }
    }

    public void InPrecursorChange(bool inPrecursor)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new UpdateInPrecursor(inPrecursor));
        }
    }

    public void DisplaySurfaceWaterChange(bool displaySurfaceWater)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new UpdateDisplaySurfaceWater(displaySurfaceWater));
        }
    }

    public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new PlayerStats(SessionId.Value, oxygen, maxOxygen, health, food, water, infectionAmount));
        }
    }

    public void BroadcastDeath(Vector3 deathPosition)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new PlayerDeathEvent(SessionId.Value, deathPosition.ToDto()));
        }
    }

    public void BroadcastSubrootChange(Optional<NitroxId> subrootId)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new SubRootChanged(SessionId.Value, subrootId));
        }
    }

    public void BroadcastEscapePodChange(Optional<NitroxId> escapePodId)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new EscapePodChanged(SessionId.Value, escapePodId));
        }
    }

    public void BroadcastWeld(NitroxId id, float healthAdded) => packetSender.Send(new WeldAction(id, healthAdded));

    public void BroadcastHeldItemChanged(NitroxId itemId, PlayerHeldItemChanged.ChangeType techType, NitroxTechType? isFirstTime)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new PlayerHeldItemChanged(SessionId.Value, itemId, techType, isFirstTime));
        }
    }

    public void BroadcastQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds) => throttledPacketSender.SendThrottled(new PlayerQuickSlotsBindingChanged(slotItemIds), (packet) => 1);

    public void BroadcastBenchChanged(NitroxId bench, BenchChanged.BenchChangeState changeState)
    {
        if (SessionId.HasValue)
        {
            packetSender.Send(new BenchChanged(SessionId.Value, bench, changeState));
        }
    }

    private GameObject CreateBodyPrototype()
    {
        GameObject prototype = Body;

        // Cheap fix for showing head, much easier since male_geo contains many different heads
        prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
        GameObject clone = Object.Instantiate(prototype, Multiplayer.Main.transform, false);
        prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

        clone.SetActive(false);
        clone.name = "RemotePlayerPrototype";

        // Removing items that are held in hand
        foreach (Transform child in clone.transform.Find($"player_view/{PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME}"))
        {
            if (!child.gameObject.name.Contains("attach1_"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        return clone;
    }
}
