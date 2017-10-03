using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Logger;
using NitroxClient.Map;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxReloader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private static readonly String DEFAULT_IP_ADDRESS = "127.0.0.1";

        public static Multiplayer main;

        private static readonly LoadedChunks loadedChunks = new LoadedChunks();
        private static readonly ChunkAwarePacketReceiver chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
        private static readonly TcpClient client = new TcpClient(chunkAwarePacketReceiver);
        public static readonly PacketSender PacketSender = new PacketSender(client);
        public static readonly Logic Logic = new Logic(PacketSender, loadedChunks, chunkAwarePacketReceiver);

        private static bool hasLoadedMonoBehaviors;

        private static readonly PlayerManager remotePlayerManager = new PlayerManager();
        private static readonly PlayerVitalsManager remotePlayerVitalsManager = new PlayerVitalsManager();
        private static readonly PlayerChatManager remotePlayerChatManager = new PlayerChatManager();

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType;

        // List of arguments that can be used in a processor:
        private static Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>()
        {
            { typeof(PlayerManager), remotePlayerManager },
            { typeof(PlayerVitalsManager), remotePlayerVitalsManager },
            { typeof(PlayerChatManager), remotePlayerChatManager },
            { typeof(PacketSender), PacketSender }
        };

        static Multiplayer()
        {
            packetProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>));
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

            main = this;
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
            Queue<Packet> packets = chunkAwarePacketReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                if (packetProcessorsByType.ContainsKey(packet.GetType()))
                {
                    try
                    {
                        PacketProcessor processor = packetProcessorsByType[packet.GetType()];
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
                PacketSender.PlayerId = (string)n.data[0];

                String ip = DEFAULT_IP_ADDRESS;

                if (n.data.Count >= 2)
                {
                    ip = (string)n.data[1];
                }

                StartMultiplayer(ip);
                InitMonoBehaviours();
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
                var opPlayer = remotePlayerManager.Find(otherPlayerId);
                if (opPlayer.IsPresent())
                {
                    Player.main.SetPosition(opPlayer.Get().body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        public void StartMultiplayer(String ipAddress)
        {
            client.Start(ipAddress);
            if (client.IsConnected())
            {
                PacketSender.Active = true;
                PacketSender.Authenticate();
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

        public void InitMonoBehaviours()
        {
            if (!hasLoadedMonoBehaviors)
            {
                this.gameObject.AddComponent<Chat>();
                this.gameObject.AddComponent<PlayerMovement>();
                this.gameObject.AddComponent<PlayerStatsBroadcaster>();
                this.gameObject.AddComponent<AnimationSender>();
                hasLoadedMonoBehaviors = true;
            }
        }
    }
}
