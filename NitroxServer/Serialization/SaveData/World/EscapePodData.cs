using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class EscapePodData
{
    [JsonProperty]
    public List<EscapePodModel> EscapePods = new();

    public static EscapePodData From(List<EscapePodModel> escapePods)
    {
        EscapePodData escapePodData = new() { EscapePods = escapePods };
        return escapePodData;
    }
}
