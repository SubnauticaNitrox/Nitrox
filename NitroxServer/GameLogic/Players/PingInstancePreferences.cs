using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Players;

[ProtoContract, JsonObject(MemberSerialization.OptIn)]
public class PingInstancePreferences
{
    [JsonProperty, ProtoMember(1)]
    public HashSet<string> HiddenSignalPings { get; } = new();

    [JsonProperty, ProtoMember(2)]
    public ThreadSafeDictionary<string, int> ColorPreferences { get; } = new();

    public PingInstancePreferences()
    {
        // Constructor for serialization
    }

    public PingInstancePreferences(HashSet<string> hiddenSignalPings, ThreadSafeDictionary<string, int> colorPreferences)
    {
        HiddenSignalPings = hiddenSignalPings;
        ColorPreferences = colorPreferences;
    }

    public override string ToString()
    {
        return $"[HiddenSignalPings: {HiddenSignalPings.Count}, colorPreferences: {ColorPreferences.Count}]";
    }

    public InitialPingInstancePreferences GetInitialData()
    {
        return new(HiddenSignalPings, ColorPreferences.ToList());
    }
}
