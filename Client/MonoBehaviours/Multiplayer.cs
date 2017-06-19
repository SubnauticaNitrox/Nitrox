using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.Communication.Packets.Processors.Base;
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
        public static bool isActive = false;
        public static PacketSender PacketSender;
        private static LoadedChunks loadedChunks;
        private static TcpClient client;
        private static ChunkAwarePacketReceiver chunkAwarePacketReceiver;

        private String playerId;

        public static Dictionary<Type, PacketProcessor> packetProcessorsByType = new Dictionary<Type, PacketProcessor>() {
            {typeof(BeginItemConstruction), new BeginItemConstructionProcessor() },
            {typeof(ChatMessage), new ChatMessageProcessor() },
            {typeof(ConstructionAmountChanged), new ConstructionAmountChangedProcessor() },
            {typeof(DroppedItem), new DroppedItemProcessor() },
            {typeof(Movement), new MovementProcessor() },
            {typeof(PickupItem), new PickupItemProcessor() }
        };

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
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
                playerId = (string)n.data[0];
                InitMultiplayerVariables();
                InitMonoBehaviours();
                isActive = true;
            }
        }

        private void InitMultiplayerVariables()
        {
            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client, playerId);

            client.Start();
            PacketSender.Authenticate();
        }
        
        public void InitMonoBehaviours()
        {
            this.gameObject.AddComponent<Chat>();
            this.gameObject.AddComponent<PlayerMovement>();
        }

        public static void AddChunk(Vector3 chunk, MonoBehaviour mb)
        {
            mb.StartCoroutine(WaitAndAddChunk(chunk));
        }

        public static void RemoveChunk(Vector3 chunk)
        {
            Int3 owningChunk = new Int3((int)chunk.x, (int)chunk.y, (int)chunk.z);
            loadedChunks.RemoveChunk(owningChunk);
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
