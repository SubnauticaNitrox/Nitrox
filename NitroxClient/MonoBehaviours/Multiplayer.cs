using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.Map;
using NitroxModel.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        private static readonly String DEFAULT_IP_ADDRESS = "127.0.0.1";

        public static bool isActive = false;
        public static PacketSender PacketSender;
        private static LoadedChunks loadedChunks;
        private static TcpClient client;
        private static ChunkAwarePacketReceiver chunkAwarePacketReceiver;

        private static PlayerGameObjectManager playerGameObjectManager = new PlayerGameObjectManager();
        private static VehicleGameObjectManager vehicleGameObjectManager = new VehicleGameObjectManager();

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType = new Dictionary<Type, PacketProcessor>() {
            {typeof(BeginItemConstruction), new BeginItemConstructionProcessor() },
            {typeof(ChatMessage), new ChatMessageProcessor() },
            {typeof(ConstructionAmountChanged), new ConstructionAmountChangedProcessor() },
            {typeof(DroppedItem), new DroppedItemProcessor() },
            {typeof(Movement), new MovementProcessor(playerGameObjectManager) },
            {typeof(PickupItem), new PickupItemProcessor() },
            {typeof(VehicleMovement), new VehicleMovementProcessor(vehicleGameObjectManager, playerGameObjectManager) }
        };

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);

            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client);
        }

        public void Update()
        {
            if (isActive)
            {
                ProcessPackets();
            }
        }

        public void ProcessPackets()
        {
            Queue<Packet> packets = chunkAwarePacketReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                PacketProcessor processor = packetProcessorsByType[packet.GetType()];

                if (processor != null)
                {
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
            if (n != null && n.data != null && n.data.Count > 0)
            {
                PacketSender.PlayerId = (string)n.data[0];

                String ip = DEFAULT_IP_ADDRESS;

                if(n.data.Count >= 2)
                {
                    ip = (string)n.data[1];
                }

                StartMultiplayer(ip);
                InitMonoBehaviours();
                isActive = true;
            }
        }

        private void StartMultiplayer(String ipAddress)
        {
            client.Start(ipAddress);
            PacketSender.Active = true;
            PacketSender.Authenticate();
        }
        
        public void InitMonoBehaviours()
        {
            this.gameObject.AddComponent<Chat>();
            this.gameObject.AddComponent<PlayerMovement>();
        }

        public static void AddChunk(Vector3 chunk, MonoBehaviour mb)
        {
            if (chunk != null && loadedChunks != null && mb != null)
            {
                mb.StartCoroutine(WaitAndAddChunk(chunk));
            }
        }

        public static void RemoveChunk(Vector3 chunk)
        {
            if (chunk != null && loadedChunks != null)
            {
                Int3 owningChunk = new Int3((int)chunk.x, (int)chunk.y, (int)chunk.z);
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
