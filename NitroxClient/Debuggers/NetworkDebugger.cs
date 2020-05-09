using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    public class NetworkDebugger : BaseDebugger
    {
        private const int PACKET_STORED_COUNT = 100;
        private readonly Dictionary<Type, int> countByType = new Dictionary<Type, int>();

        private readonly List<string> filter = new List<string> { nameof(Movement), nameof(EntityTransformUpdates), nameof(PlayerStats), nameof(CellEntities), nameof(VehicleMovement) };
        private readonly List<PacketDebugWrapper> packets = new List<PacketDebugWrapper>(PACKET_STORED_COUNT);

        // vs blacklist
        private bool isWhitelist;
        private int receivedCount;
        private Vector2 scrollPosition;
        private int sentCount;

        public NetworkDebugger() : base(600, null, KeyCode.N, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            ActiveTab = AddTab("All", RenderTabPackets);
            AddTab("Sent", RenderTabSentPackets);
            AddTab("Received", RenderTabReceivedPackets);
            AddTab("Type Count", RenderTabTypeCount);
            AddTab("Filter", RenderTabFilter);
        }

        public void PacketSent(Packet packet)
        {
            AddPacket(packet, true);
            sentCount++;
        }

        public void PacketReceived(Packet packet)
        {
            AddPacket(packet, false);
            receivedCount++;
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
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.BOTH);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabSentPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.SENT);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabReceivedPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                RenderPacketList(ToRender.RECEIVED);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTabTypeCount()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");

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
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
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
                        IShortString hasShortString = wrapper.Packet as IShortString;
                        GUILayout.Label(hasShortString != null ? hasShortString.ToShortString() : wrapper.Packet.ToString());
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

            int count;
            if (countByType.TryGetValue(packetType, out count))
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
}
