using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    [ExcludeFromCodeCoverage]
    public class NetworkDebugger : BaseDebugger, INetworkDebugger
    {
        private const int PACKET_STORED_COUNT = 100;
        private readonly Dictionary<Type, int> countByType = new Dictionary<Type, int>();

        private readonly List<string> filter = new()
        {
            nameof(PlayerMovement), nameof(EntityTransformUpdates), nameof(PlayerStats), nameof(SpawnEntities), nameof(VehicleMovement), nameof(PlayerCinematicControllerCall),
            nameof(FMODAssetPacket), nameof(FMODEventInstancePacket), nameof(FMODCustomEmitterPacket), nameof(FMODStudioEmitterPacket), nameof(FMODCustomLoopingEmitterPacket), 
            nameof(SimulationOwnershipChange), nameof(CellVisibilityChanged)
        };
        private readonly List<PacketDebugWrapper> packets = new List<PacketDebugWrapper>(PACKET_STORED_COUNT);

        // vs blacklist
        private bool isWhitelist;
        private Vector2 scrollPosition;

        private int receivedCount;
        private int sentCount;

        private uint receivedBytes;
        private uint sentBytes;

        public NetworkDebugger() : base(600, null, KeyCode.N, true, false, false, GUISkinCreationOptions.DERIVEDCOPY, 330)
        {
            ActiveTab = AddTab("All", RenderTabPackets);
            AddTab("Sent", RenderTabSentPackets);
            AddTab("Received", RenderTabReceivedPackets);
            AddTab("Type Count", RenderTabTypeCount);
            AddTab("Filter", RenderTabFilter);
        }

        public void PacketSent(Packet packet, int byteSize)
        {
            AddPacket(packet, true);
            sentCount++;
            sentBytes += (uint)byteSize;
        }

        public void PacketReceived(Packet packet, int byteSize)
        {
            AddPacket(packet, false);
            receivedCount++;
            receivedBytes += (uint)byteSize;
        }

        protected override void OnSetSkin(GUISkin skin)
        {
            base.OnSetSkin(skin);

            skin.SetCustomStyle("packet-type-down",
                                skin.label,
                                s =>
                                {
                                    s.normal = new GUIStyleState { textColor = Color.green };
                                    s.fontStyle = FontStyle.Bold;
                                    s.alignment = TextAnchor.MiddleLeft;
                                });

            skin.SetCustomStyle("packet-type-up",
                                skin.label,
                                s =>
                                {
                                    s.normal = new GUIStyleState { textColor = Color.red };
                                    s.fontStyle = FontStyle.Bold;
                                    s.alignment = TextAnchor.MiddleLeft;
                                });
        }

        private void RenderTabPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                RenderPacketTotals();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.BOTH);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabSentPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                RenderPacketTotals();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.SENT);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabReceivedPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                RenderPacketTotals();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.RECEIVED);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabTypeCount()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                RenderPacketTotals();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                foreach (KeyValuePair<Type, int> kv in countByType.OrderBy(e => -e.Value)) // descending
                {
                    GUILayout.Label($"{kv.Key.Name}: {kv.Value}");
                }
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabFilter()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                RenderPacketTotals();
                using (new GUILayout.HorizontalScope())
                {
                    isWhitelist = GUILayout.Toggle(isWhitelist, "Is Whitelist");
                    if (GUILayout.Button("Clear"))
                    {
                        filter.Clear();
                    }
                }

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                for (int i = 0; i < filter.Count; i++)
                {
                    filter[i] = GUILayout.TextField(filter[i]);
                }
                string n = GUILayout.TextField("");
                if (n != "")
                {
                    filter.Add(n);
                }
                GUILayout.EndScrollView();
            }
        }

        private void RenderPacketTotals()
        {
            GUILayout.Label($"Sent: {sentCount} ({sentBytes.AsByteUnitText()}) - Received: {receivedCount} ({receivedBytes.AsByteUnitText()})");
        }

        private void RenderPacketList(ToRender toRender)
        {
            bool isSentList = toRender.HasFlag(ToRender.SENT);
            bool isReceiveList = toRender.HasFlag(ToRender.RECEIVED);
            PacketPrefixer prefixer = isSentList && isReceiveList ? (PacketPrefixer)PacketDirectionPrefixer : PacketNoopPrefixer;

            for (int i = packets.Count - 1; i >= 0; i--)
            {
                PacketDebugWrapper wrapper = packets[i];
                if (wrapper.IsSent && !isSentList)
                {
                    continue;
                }
                if (!wrapper.IsSent && !isReceiveList)
                {
                    continue;
                }

                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        wrapper.ShowDetails = GUILayout.Toggle(wrapper.ShowDetails, "", GUILayout.Width(20), GUILayout.Height(20));
                        GUILayout.Label($"{prefixer(wrapper)}{wrapper.Packet.GetType().FullName}", wrapper.IsSent ? "packet-type-up" : "packet-type-down");

                        packets[i] = wrapper; // Store again because value-type
                    }

                    if (wrapper.ShowDetails)
                    {
                        GUILayout.Label(wrapper.Packet.ToString());
                    }
                }
            }
        }

        private void AddPacket(Packet packet, bool isSent)
        {
            Type packetType = packet.GetType();
            if (isWhitelist == filter.Contains(packetType.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                packets.Add(new PacketDebugWrapper(packet, isSent, false));
                if (packets.Count > PACKET_STORED_COUNT)
                {
                    packets.RemoveAt(0);
                }
            }

            if (countByType.TryGetValue(packetType, out int count))
            {
                countByType[packetType] = count + 1;
            }
            else
            {
                countByType.Add(packetType, 1);
            }
        }

        private string PacketDirectionPrefixer(PacketDebugWrapper wrapper) => $"{(wrapper.IsSent ? "↑" : "↓")} - ";

        private string PacketNoopPrefixer(PacketDebugWrapper wraper) => "";

        private delegate string PacketPrefixer(PacketDebugWrapper wrapper);

        private struct PacketDebugWrapper
        {
            public readonly Packet Packet;
            public readonly bool IsSent;

            public bool ShowDetails { get; set; }

            public PacketDebugWrapper(Packet packet, bool isSent, bool showDetails)
            {
                IsSent = isSent;
                Packet = packet;
                ShowDetails = showDetails;
            }
        }

        [Flags]
        private enum ToRender
        {
            SENT = 1,
            RECEIVED = 2,
            BOTH = SENT | RECEIVED
        }
    }

    public interface INetworkDebugger
    {
        void PacketSent(Packet packet, int size);
        void PacketReceived(Packet packet, int size);
    }
}
