using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDALogEntryAddProcessor : ClientPacketProcessor<PDALogEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDALogEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDALogEntryAdd packet)
        {
            using (packetSender.Suppress<PDALogEntryAddProcessor>())
            {
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));


                if (!entries.ContainsKey(packet.Key))
                {
                    PDALog.EntryData entryData;
                    if (!PDALog.GetEntryData(packet.Key, out entryData))
                    {
                        UWE.Utils.LogReportFormat("PDALog : Add() : EntryData for key='{0}' is not found!", new object[]
                        {
                    packet.Key
                        });
                        entryData = new PDALog.EntryData();
                        entryData.key = packet.Key;
                        entryData.type = PDALog.EntryType.Invalid;
                    }
                    PDALog.Entry entry = new PDALog.Entry();
                    entry.data = entryData;
                    entry.timestamp = packet.Timestamp;
                    entries.Add(entryData.key, entry);
                }
            }
        }
    }
}
