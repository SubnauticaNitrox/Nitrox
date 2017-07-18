using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
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

        public static Logic Logic;
        public static PacketSender PacketSender; //TODO: migrate logic out of and remove as member variable
        private static LoadedChunks loadedChunks;
        private static TcpClient client;
        private static ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        private static bool hasLoadedMonoBehaviors;

        private static PlayerGameObjectManager playerGameObjectManager = new PlayerGameObjectManager();

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType = new Dictionary<Type, PacketProcessor>() {
            {typeof(PlaceBasePiece), new PlaceBasePieceProcessor() },
            {typeof(PlaceFurniture), new PlaceFurnitureProcessor() },
            {typeof(AnimationChangeEvent), new AnimationProcessor(playerGameObjectManager) },
            {typeof(ConstructorBeginCrafting), new ConstructorBeginCraftingProcessor() },
            {typeof(ChatMessage), new ChatMessageProcessor() },
            {typeof(ConstructionAmountChanged), new ConstructionAmountChangedProcessor() },
            {typeof(ConstructionCompleted), new ConstructionCompletedProcessor() },
            {typeof(Disconnect), new DisconnectProcessor(playerGameObjectManager) },
            {typeof(DroppedItem), new DroppedItemProcessor() },
            {typeof(Movement), new MovementProcessor(playerGameObjectManager) },
            {typeof(PickupItem), new PickupItemProcessor() },
            {typeof(VehicleMovement), new VehicleMovementProcessor(playerGameObjectManager) },
            {typeof(ItemPosition), new ItemPositionProcessor() }
        };

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);

            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client);
            Logic = new Logic(PacketSender);
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
            if (client.IsConnected())
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
            if (client.IsConnected())
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
