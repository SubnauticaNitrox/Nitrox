using System;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerPingManager : MonoBehaviour
{
    private const float MAX_PING_DISTANCE = 1000f;
    private const float MIN_PING_DISTANCE = 3f;
    private const float MIN_PING_COOLDOWN = 0.5f;
    private const int MAX_ACTIVE_PINGS = 3;
    
    private static PlayerPingManager instance;
    
    private IPacketSender packetSender;
    private LocalPlayer localPlayer;
    private float lastPingTime;
    private Camera mainCamera;
    private readonly Dictionary<SessionId, List<PlayerPing>> playerPings = new();
    private readonly List<PlayerPing> localPlayerPings = new();

    public void Awake()
    {
        instance = this;
        packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        localPlayer = this.Resolve<LocalPlayer>();
    }

    private void Update()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }

        if (Input.GetMouseButtonDown(2) && CanCreatePing())
        {
            TryCreatePing();
        }
    }

    private bool CanCreatePing()
    {
        if (Time.time - lastPingTime < MIN_PING_COOLDOWN)
        {
            return false;
        }

        if (!Player.main || !AvatarInputHandler.main.IsEnabled())
        {
            return false;
        }

        return true;
    }

    private void TryCreatePing()
    {
        if (!mainCamera)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~(LayerMask.GetMask("UI", "Ignore Raycast"));
        RaycastHit[] hits = Physics.RaycastAll(ray, MAX_PING_DISTANCE, layerMask);
        
        if (hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.isTrigger || hit.distance < MIN_PING_DISTANCE)
                {
                    continue;
                }
                
                Vector3 pingPosition = hit.point + hit.normal * 0.5f;
                CreatePing(pingPosition);
                return;
            }
        }
    }

    private void CreatePing(Vector3 position)
    {
        lastPingTime = Time.time;

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
        string playerName = localPlayer.PlayerName;
        SessionId sessionId = localPlayer.SessionId.Value;
        Color playerColor = localPlayer.PlayerSettings.PlayerColor.ToUnity();

        PlayerPing newPing = PlayerPing.SpawnPlayerPing(position.ToDto(), playerName, pingId, playerColor);
        if (newPing)
        {
            localPlayerPings.Add(newPing);
        }

        packetSender.Send(new PlayerPingCreated(sessionId, playerName, position.ToDto(), pingId));
    }

    public static void CreateRemotePing(SessionId sessionId, string playerName, NitroxVector3 position, NitroxId pingId)
    {
        if (!instance)
        {
            return;
        }
        
        instance.CreateRemotePingInternal(sessionId, playerName, position, pingId);
    }
    
    private void CreateRemotePingInternal(SessionId sessionId, string playerName, NitroxVector3 position, NitroxId pingId)
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
        
        PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
        Optional<RemotePlayer> remotePlayerOptional = playerManager.Find(sessionId);
        
        Color playerColor = remotePlayerOptional.HasValue 
            ? remotePlayerOptional.Value.PlayerSettings.PlayerColor.ToUnity() 
            : Color.yellow;
        
        PlayerPing newPing = PlayerPing.SpawnPlayerPing(position, playerName, pingId, playerColor);
        if (newPing)
        {
            pings.Add(newPing);
        }
    }
}
