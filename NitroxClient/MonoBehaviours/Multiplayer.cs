using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxClient.Map;
using NitroxModel.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private static readonly String DEFAULT_IP_ADDRESS = "127.0.0.1";
        
        public static PacketSender PacketSender;
        private static LoadedChunks loadedChunks;
        private static TcpClient client;
        private static ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        private static bool hasLoadedMonoBehaviors;

        private static PlayerGameObjectManager playerGameObjectManager = new PlayerGameObjectManager();
        private static MultiplayerObjectManager multiplayerObjectManager = new MultiplayerObjectManager();

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType = new Dictionary<Type, PacketProcessor>() {
            {typeof(BeginItemConstruction), new BeginItemConstructionProcessor() },
            {typeof(ChatMessage), new ChatMessageProcessor() },
            {typeof(ConstructionAmountChanged), new ConstructionAmountChangedProcessor() },
            {typeof(Disconnect), new DisconnectProcessor(playerGameObjectManager) },
            {typeof(DroppedItem), new DroppedItemProcessor(multiplayerObjectManager) },
            {typeof(Movement), new MovementProcessor(playerGameObjectManager) },
            {typeof(PickupItem), new PickupItemProcessor() },
            {typeof(VehicleMovement), new VehicleMovementProcessor(multiplayerObjectManager, playerGameObjectManager) },
            {typeof(ConstructorBeginCrafting), new ConstructorBeginCraftingProcessor(multiplayerObjectManager) },
            {typeof(ItemPosition), new ItemPositionProcessor(multiplayerObjectManager) }
        };

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);

            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client, multiplayerObjectManager);
        }

        public void Update()
        {
            if (client != null && client.isConnected())
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
                    PacketProcessor processor = packetProcessorsByType[packet.GetType()];
                    processor.ProcessPacket(packet);
                }
                else
                {
                    Console.WriteLine("No packet processor for the given type: " + packet.GetType());
                }
            }
        }
        
        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            if (client.isConnected())
            {
                ErrorMessage.AddMessage("Already connected to a server");
            } 
            else if (n?.data?.Count > 0)
            {
                PacketSender.PlayerId = (string)n.data[0];

                String ip = DEFAULT_IP_ADDRESS;

                if(n.data.Count >= 2)
                {
                    ip = (string)n.data[1];
                }

                StartMultiplayer(ip);
                InitMonoBehaviours();
            } 
            else
            {
                ErrorMessage.AddMessage("Command syntax: mplayer USERNAME [SERVERIP]");
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
                var otherPlayer = playerGameObjectManager.FindPlayerGameObject(otherPlayerId);
                if (otherPlayer != null)
                {
                    Player.main.SetPosition(otherPlayer.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        private void StartMultiplayer(String ipAddress)
        {
            client.Start(ipAddress);
            PacketSender.Active = true;
            PacketSender.Authenticate();
        }

        private void StopMultiplayer()
        {
            if (client.isConnected())
            {
                client.Stop();
                PacketSender.Active = false;
            }
        }
        
        public void InitMonoBehaviours()
        {
            if (!hasLoadedMonoBehaviors)
            {
                this.gameObject.AddComponent<Chat>();
                this.gameObject.AddComponent<PlayerMovement>();
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
