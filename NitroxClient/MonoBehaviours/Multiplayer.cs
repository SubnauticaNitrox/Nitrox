using System;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxClient.Map;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxReloader;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";

        public static Multiplayer Main;

        public static event Action OnBeforeMultiplayerStart;

        public static DiscordController DiscordRP;

        private static readonly VisibleCells visibleCells = new VisibleCells();
        private static readonly DeferringPacketReceiver packetReceiver = new DeferringPacketReceiver(visibleCells);
        private static readonly TcpClient client = new TcpClient(packetReceiver);
        public static readonly PacketSender PacketSender = new PacketSender(client);
        public static readonly Logic Logic = new Logic(PacketSender, visibleCells, packetReceiver);

        private static bool hasLoadedMonoBehaviors;

        private static readonly PlayerManager remotePlayerManager = new PlayerManager();
        private static readonly PlayerVitalsManager remotePlayerVitalsManager = new PlayerVitalsManager();
        private static readonly PlayerChatManager remotePlayerChatManager = new PlayerChatManager();

        public static Dictionary<Type, PacketProcessor> PacketProcessorsByType;

        // List of arguments that can be used in a processor:
        private static Dictionary<Type, object> processorArguments = new Dictionary<Type, object>
        {
            { typeof(PlayerManager), remotePlayerManager },
            { typeof(PlayerVitalsManager), remotePlayerVitalsManager },
            { typeof(PlayerChatManager), remotePlayerChatManager },
            { typeof(PacketSender), PacketSender }
        };

        static Multiplayer()
        {
            PacketProcessorsByType = PacketProcessor.GetProcessors(processorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>));
        }

        public static void RemoveAllOtherPlayers()
        {
            remotePlayerManager.RemoveAllPlayers();
        }

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);
            DevConsole.RegisterConsoleCommand(this, "discord", false);

            Main = this;
        }

        public void Update()
        {
            Reloader.ReloadAssemblies();
            if (client != null && client.IsConnected())
            {
                ProcessPackets();
            }
        }

        public void ProcessPackets()
        {
            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                if (PacketProcessorsByType.ContainsKey(packet.GetType()))
                {
                    try
                    {
                        PacketProcessor processor = PacketProcessorsByType[packet.GetType()];
                        processor.ProcessPacket(packet, null);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error processing packet: " + packet, ex);
                    }
                }
                else
                {
                    Log.Debug("No packet processor for the given type: " + packet.GetType());
                }
            }
        }

        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            if (client.IsConnected())
            {
                Log.InGame("Already connected to a server");
            }
            else if (n?.data?.Count > 0)
            {
                StartMultiplayer(n.data.Count >= 2 ? (string)n.data[1] : DEFAULT_IP_ADDRESS, (string)n.data[0]);
            }
            else
            {
                Log.InGame("Command syntax: mplayer USERNAME [SERVERIP]");
            }
        }

        public void OnConsoleCommand_disconnect(NotificationCenter.Notification n)
        {
            if (n != null)
            {
                StopMultiplayer(); // TODO: More than just disconnect (clean up injections or something)
            }
        }

        public void OnConsoleCommand_warpto(NotificationCenter.Notification n)
        {
            if (n?.data?.Count > 0)
            {
                string otherPlayerId = (string)n.data[0];
                Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(otherPlayerId);
                if (opPlayer.IsPresent())
                {
                    Player.main.SetPosition(opPlayer.Get().Body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        public void OnConsoleCommand_discord(NotificationCenter.Notification n)
        {
            DiscordRP.RequestCallbackTest();
        }

        public void StartMultiplayer(string ipAddress, string playerName)
        {
            OnBeforeMultiplayerStart();

            PacketSender.PlayerId = playerName;
            StartMultiplayer(ipAddress);
            InitMonoBehaviours(ipAddress);
        }

        public void StartMultiplayer(string ipAddress)
        {
            client.Start(ipAddress);
            if (client.IsConnected())
            {
                PacketSender.Active = true;
                Multiplayer.Logic.Player.Authenticate(PacketSender.PlayerId);
                Log.InGame("Connected to server");
            }
            else
            {
                Log.InGame("Unable to connect to server");
            }
        }

        private void StopMultiplayer()
        {
            if (client.IsConnected())
            {
                client.Stop();
            }
        }

        public void InitMonoBehaviours(string ipAddress)
        {
            if (!hasLoadedMonoBehaviors)
            {
                gameObject.AddComponent<Chat>();
                gameObject.AddComponent<PlayerMovement>();
                gameObject.AddComponent<PlayerStatsBroadcaster>();
                gameObject.AddComponent<AnimationSender>();
                gameObject.AddComponent<EntityPositionBroadcaster>();
                DiscordRP = gameObject.AddComponent<DiscordController>();
                InitDiscordRichPresence(ipAddress);

                hasLoadedMonoBehaviors = true;
            }
        }

        private void InitDiscordRichPresence(string ipAddress)
        {
            DiscordRP.Presence.state = "In Game";
            DiscordRP.Presence.partyId = "<Server Name>";
            DiscordRP.Presence.partySize = 1 + remotePlayerManager.GetPlayerCount();
            DiscordRP.Presence.partyMax = 42;
            DiscordRP.Presence.joinSecret = ipAddress;
            DiscordRP.Presence.instance = false;
            DiscordRP.UpdatePresence();
        }
    }
}
