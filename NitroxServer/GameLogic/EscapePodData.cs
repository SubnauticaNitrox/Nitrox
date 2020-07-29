using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodData
    {
        [ProtoMember(1)]
        public List<NitroxObject> EscapePods;

        public static EscapePodData From(List<EscapePodModel> escapePods)
        {
            EscapePodData escapePodData = new EscapePodData { EscapePods = escapePods.Select(e => e.NitroxObject).ToList() };
            return escapePodData;
        }
    }
}
