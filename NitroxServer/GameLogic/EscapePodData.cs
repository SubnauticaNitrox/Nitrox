using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class EscapePodData
    {
        [JsonProperty, ProtoMember(1)]
        public List<EscapePodModel> EscapePods;

        public static EscapePodData From(List<EscapePodModel> escapePods)
        {
            EscapePodData escapePodData = new EscapePodData { EscapePods = escapePods };
            return escapePodData;
        }
    }
}
