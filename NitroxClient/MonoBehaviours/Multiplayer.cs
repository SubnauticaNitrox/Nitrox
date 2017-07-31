﻿using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Logger;
using NitroxClient.Map;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private static readonly String DEFAULT_IP_ADDRESS = "127.0.0.1";

        public static Multiplayer main;

        private static LoadedChunks loadedChunks = new LoadedChunks();
        private static ChunkAwarePacketReceiver chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
        private static TcpClient client = new TcpClient(chunkAwarePacketReceiver);
        public static PacketSender PacketSender = new PacketSender(client);
        public static Logic Logic = new Logic(PacketSender);

        private static bool hasLoadedMonoBehaviors;

        private static PlayerManager remotePlayerManager = new PlayerManager();

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType;

        // List of arguments that can be used in a processor:
        private static Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>()
        {
            { typeof(PlayerManager), remotePlayerManager },
            { typeof(PacketSender), PacketSender }
        };

        static Multiplayer()
        {
            packetProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>));
        }

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);
            ClientLogger.setLogLocation(ClientLogger.LOG_CONSOLE | ClientLogger.LOG_ERRORMESSAGE);

            this.gameObject.AddComponent<PlayerMovement>();

            main = this;
        }

        public void Update()
        {
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
                        Console.WriteLine("Error processing packet: " + packet + ": " + ex);
                    }
                }
                else
                {
                    ClientLogger.DebugLine("No packet processor for the given type: " + packet.GetType());
                }
            }
        }

        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            if (client.IsConnected())
            {
                ClientLogger.WriteLine("Already connected to a server");
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
                ClientLogger.WriteLine("Command syntax: mplayer USERNAME [SERVERIP]");
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
                var otherPlayer = remotePlayerManager.FindPlayer(otherPlayerId);
                if (otherPlayer != null)
                {
                    Player.main.SetPosition(otherPlayer.body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        public void StartMultiplayer(String ipAddress)
        {
            client.Start(ipAddress);
            PacketSender.Active = true;
            PacketSender.Authenticate();
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
                this.gameObject.AddComponent<AnimationSender>();
                hasLoadedMonoBehaviors = true;
            }
        }

        public static void AddChunk(Vector3 chunk, MonoBehaviour mb)
        {
            if (chunk != null && loadedChunks != null && mb != null)
            {
                mb.StartCoroutine(WaitAndAddChunk(chunk));
            }
        }

        public static void RemoveChunk(VoxelandChunk chunk)
        {
            if (chunk?.transform != null && loadedChunks != null)
            {
                Int3 owningChunk = ApiHelper.Int3(chunk.transform.position);
                loadedChunks.RemoveChunk(owningChunk);
            }
        }

        private static IEnumerator WaitAndAddChunk(Vector3 chunk)
        {
            yield return new WaitForSeconds(0.5f);
            Int3 owningChunk = new Int3((int)chunk.x, (int)chunk.y, (int)chunk.z);
            loadedChunks.AddChunk(owningChunk);
            chunkAwarePacketReceiver.ChunkLoaded(owningChunk);
        }
    }
}
