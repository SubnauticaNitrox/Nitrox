using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodData
    {
        [ProtoMember(1)]
        public List<EscapePodModel> EscapePods;

        public static EscapePodData from(List<EscapePodModel> escapePods)
        {
            EscapePodData escapePodData = new EscapePodData();
            escapePodData.EscapePods = escapePods;

            return escapePodData;
        }
    }
}
