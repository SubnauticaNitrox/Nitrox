using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    public class NetworkDebugger : BaseDebugger
    {
        private const int PACKET_STORED_COUNT = 100;

        private List<string> filter = new List<string>()
        {
            "Movement",
            "EntityTransformUpdates",
        };

        // vs blacklist
        private bool isWhitelist = false;

        public static NetworkDebugger Instance { get; private set; }
        private List<DebugPacket> packets = new List<DebugPacket>();
        private List<bool> details = new List<bool>();
        private Vector2 scrollPosition;
        private int sentCount = 0;
        private int receivedCount = 0;
        private Dictionary<Type, int> countByType = new Dictionary<Type, int>();

        public NetworkDebugger() : base(400, null, KeyCode.N, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            Instance = this;
            ActiveTab = AddTab("All", RenderTabPackets);
            AddTab("Sent", RenderSentTabPackets);
            AddTab("Received", RenderReceivedTabPackets);
            AddTab("Type Count", RenderTypeCountTab);
            AddTab("Filter", RenderFilter);
        }

        private void RenderTabPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                Render(ToRender.BOTH);
                GUILayout.EndScrollView();
            }
        }

        private void RenderSentTabPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                Render(ToRender.SENT);
                GUILayout.EndScrollView();
            }
        }

        private void RenderReceivedTabPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                Render(ToRender.RECEIVED);
                GUILayout.EndScrollView();
            }
        }

        private void RenderTypeCountTab()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                foreach (KeyValuePair<Type, int> kv in countByType.OrderBy(e => -e.Value))// descending
                {
                    GUILayout.Label($"{kv.Key.Name}: {kv.Value}");
                }
                GUILayout.EndScrollView();
            }
        }

        private void RenderFilter()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Sent: {sentCount} - Received: {receivedCount}");
                GUILayout.BeginHorizontal();
                isWhitelist = GUILayout.Toggle(isWhitelist, "Is Whitelist");
                if(GUILayout.Button("Clear"))
                {
                    filter.Clear();
                }
                GUILayout.EndHorizontal();

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

        private void Render(ToRender toRender)
        {
            for (int i = packets.Count - 1; i >= 0; i--)
            {
                DebugPacket debugPacket = packets[i];
                if(debugPacket.IsSent ? (toRender & ToRender.SENT) != 0 : (toRender & ToRender.RECEIVED) != 0)
                {
                    using (new GUILayout.VerticalScope("Box"))
                    {

                        string direction = debugPacket.IsSent ? "Sent" : "Received";
                        GUILayout.Label($"{direction} - {debugPacket.Packet.GetType()}");
                        details[i] = GUILayout.Toggle(details[i], "Show content");
                        if(details[i])
                        {
                            if (debugPacket.Packet is IDebugPacket)
                            {
                                GUILayout.Label(((IDebugPacket)debugPacket.Packet).GetSDebugString());
                            }
                            else
                            {
                                GUILayout.Label(debugPacket.Packet.ToString());
                            }
                        }
                        packets[i] = debugPacket;// store the details var
                    }
                }
            }
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

        private void AddPacket(Packet packet, bool isSent)
        {
            if(isWhitelist == filter.Contains(packet.GetType().Name))
            {
                details.Add(false);
                packets.Add(new DebugPacket(isSent, packet));
                if (packets.Count > PACKET_STORED_COUNT)
                {
                    details.RemoveAt(0);
                    packets.RemoveAt(0);
                }
            }
            if(!countByType.ContainsKey(packet.GetType()))
            {
                countByType.Add(packet.GetType(), 1);
            }
            else
            {
                countByType[packet.GetType()]++;
            }
        }

        private struct DebugPacket
        {
            public readonly bool IsSent;

            public readonly Packet Packet;

            public DebugPacket(bool isSent, Packet packet)
            {
                IsSent = isSent;
                Packet = packet;
            }
        }

        enum ToRender { SENT = 1, RECEIVED = 2, BOTH = 3 }
    }
}
