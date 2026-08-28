using System;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours.Gui.InGame;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

internal sealed class PlayerPingManager : MonoBehaviour
{
    private const float MAX_PING_DISTANCE = 250f;
    private const float MIN_PING_COOLDOWN = 0.5f;
    private const int MAX_ACTIVE_PINGS = 3;
    public const string PING_OK_SOUND = "event:/sub/cyclops/sonar";
    public const string PING_FAIL_SOUND = "event:/tools/divereel/breadcrum";

    private static PlayerPingManager instance = null!;

    private RaycastHit? queuedRaycastHit;
    private IPacketSender packetSender = null!;
    private LocalPlayer localPlayer = null!;
    private float lastPingTime;
    private readonly Dictionary<SessionId, List<PlayerPing>> playerPings = [];
    private readonly List<PlayerPing> localPlayerPings = new(MAX_ACTIVE_PINGS);

    public void Awake()
    {
        if (instance)
        {
            Log.Error($"Tried to initialize a second instance of singleton {nameof(PlayerPingManager)}:{Environment.NewLine}{Environment.StackTrace}");
            Destroy(this);
            return;
        }
        instance = this;
        packetSender = this.Resolve<IPacketSender>();
        localPlayer = this.Resolve<LocalPlayer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            queuedRaycastHit = RaycastHelper.GetClosestHitFromAim(MAX_PING_DISTANCE);
            if (queuedRaycastHit == null)
            {
                // Invalid ping raycast so we play an error sound.
                FMODEmitterController.PlayEventOneShot(PING_FAIL_SOUND, 3, Player.main.transform.position);
            }
        }
        if (queuedRaycastHit is { } hit && CanCreatePing())
        {
            queuedRaycastHit = null;
            CreatePing(hit.point + hit.normal * 0.5f, hit.collider.gameObject);
        }
    }

    private bool CanCreatePing()
    {
        if (Time.realtimeSinceStartup - lastPingTime < MIN_PING_COOLDOWN)
        {
            return false;
        }
        if (!Player.main || !AvatarInputHandler.main.IsEnabled())
        {
            return false;
        }

        return true;
    }

    private void CreatePing(Vector3 position, GameObject entityHit)
    {
        if (!entityHit)
        {
            return;
        }
        lastPingTime = Time.realtimeSinceStartup;
        if (!localPlayer.SessionId.HasValue)
        {
            return;
        }

        localPlayerPings.RemoveAll(ping => !ping);

        if (localPlayerPings.Count >= MAX_ACTIVE_PINGS)
        {
            PlayerPing oldestPing = localPlayerPings[0];
            if (oldestPing)
            {
                Destroy(oldestPing.gameObject);
            }
            localPlayerPings.RemoveAt(0);
        }

        NitroxId pingId = new();
        SessionId sessionId = localPlayer.SessionId.Value;
        Color playerColor = localPlayer.PlayerSettings.PlayerColor.ToUnity();
        string label = $"{localPlayer.PlayerName} pinged {entityHit.GetFriendlyName()}";

        PlayerPing newPing = PlayerPing.SpawnPlayerPing(position.ToDto(), label, pingId, playerColor);
        if (newPing)
        {
            localPlayerPings.Add(newPing);
        }

        packetSender.Send(new PlayerPingCreated(sessionId, label, position.ToDto(), pingId));
    }

    public static void CreateRemotePing(SessionId sessionId, string labelText, NitroxVector3 position, NitroxId pingId)
    {
        if (!instance)
        {
            return;
        }
        
        instance.CreateRemotePingInternal(sessionId, labelText, position, pingId);
    }
    
    private void CreateRemotePingInternal(SessionId sessionId, string labelText, NitroxVector3 position, NitroxId pingId)
    {
        if (!playerPings.TryGetValue(sessionId, out List<PlayerPing> pings))
        {
            pings = new List<PlayerPing>();
            playerPings[sessionId] = pings;
        }
        
        pings.RemoveAll(ping => !ping);
        
        if (pings.Count >= MAX_ACTIVE_PINGS)
        {
            PlayerPing oldestPing = pings[0];
            if (oldestPing)
            {
                Destroy(oldestPing.gameObject);
            }
            pings.RemoveAt(0);
        }
        
        PlayerManager playerManager = this.Resolve<PlayerManager>();
        Optional<RemotePlayer> remotePlayerOptional = playerManager.Find(sessionId);
        
        Color playerColor = remotePlayerOptional.HasValue 
            ? remotePlayerOptional.Value.PlayerSettings.PlayerColor.ToUnity() 
            : Color.yellow;
        
        PlayerPing newPing = PlayerPing.SpawnPlayerPing(position, labelText, pingId, playerColor);
        if (newPing)
        {
            pings.Add(newPing);
        }
    }
}
