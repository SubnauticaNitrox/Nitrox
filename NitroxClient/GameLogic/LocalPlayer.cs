using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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
    public ushort? PlayerId => multiplayerSession?.Reservation?.PlayerId;
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
#if SUBNAUTICA
        playerModel = new Lazy<GameObject>(() => Body.RequireGameObject("player_view"));
#elif BELOWZERO
        playerModel = new Lazy<GameObject>(() => Body.RequireGameObject("player_view_female"));
#endif
        bodyPrototype = new Lazy<GameObject>(CreateBodyPrototype);
        Permissions = Perms.PLAYER;
        IntroCinematicMode = IntroCinematicMode.NONE;
        KeepInventoryOnDeath = false;
    }

    public void BroadcastLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
    {
        if (!PlayerId.HasValue)
        {
            return;
        }

        PlayerMovement playerMovement = new(PlayerId.Value, location.ToDto(), velocity.ToDto(), bodyRotation.ToDto(), aimingRotation.ToDto());

        packetSender.Send(playerMovement);
    }

    public void AnimationChange(AnimChangeType type, AnimChangeState state)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new AnimationChangeEvent(PlayerId.Value, (int)type, (int)state));
        }
    }

#if SUBNAUTICA
    public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new PlayerStats(PlayerId.Value, oxygen, maxOxygen, health, food, water, infectionAmount));
        }
    }
#elif BELOWZERO
    public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new PlayerStats(PlayerId.Value, oxygen, maxOxygen, health, food, water));
        }
    }
#endif

    public void BroadcastDeath(Vector3 deathPosition)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new PlayerDeathEvent(PlayerId.Value, deathPosition.ToDto()));
        }
    }

    public void BroadcastSubrootChange(Optional<NitroxId> subrootId)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new SubRootChanged(PlayerId.Value, subrootId));
        }
    }

#if SUBNAUTICA
    public void BroadcastEscapePodChange(Optional<NitroxId> escapePodId)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new EscapePodChanged(PlayerId.Value, escapePodId));
        }
    }
#endif

    public void BroadcastWeld(NitroxId id, float healthAdded) => packetSender.Send(new WeldAction(id, healthAdded));

    public void BroadcastHeldItemChanged(NitroxId itemId, PlayerHeldItemChanged.ChangeType techType, NitroxTechType isFirstTime)
    {
        if (PlayerId.HasValue)
        {
            packetSender.Send(new PlayerHeldItemChanged(PlayerId.Value, itemId, techType, isFirstTime));
        }
    }

    public void BroadcastQuickSlotsBindingChanged(Optional<NitroxId>[] slotItemIds) => throttledPacketSender.SendThrottled(new PlayerQuickSlotsBindingChanged(slotItemIds), (packet) => 1);

    private GameObject CreateBodyPrototype()
    {
        GameObject prototype = Body;

        // Cheap fix for showing head, much easier since male_geo contains many different heads
#if SUBNAUTICA
        prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
        GameObject clone = Object.Instantiate(prototype, Multiplayer.Main.transform, false);
        prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
#elif BELOWZERO
        prototype.GetComponentInParent<Player>().staticHead.shadowCastingMode = ShadowCastingMode.On;
        GameObject clone = Object.Instantiate(prototype);
        prototype.GetComponentInParent<Player>().staticHead.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
#endif

        clone.SetActive(false);
        clone.name = "RemotePlayerPrototype";

        // Removing items that are held in hand
#if SUBNAUTICA
        foreach (Transform child in clone.transform.Find($"player_view/{PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME}"))
#elif BELOWZERO
        foreach (Transform child in clone.transform.Find($"player_view_female/{PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME}"))
#endif
        {
            if (!child.gameObject.name.Contains("attach1_"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        return clone;
    }
}
