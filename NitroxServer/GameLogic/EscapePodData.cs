using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodData
    {
        [ProtoMember(1)]
        public List<EscapePodModel> SerializedEscapePods
        {
            get
            {
                lock (EscapePods)
                {
                    return EscapePods;
                }
            }
            set
            {
                EscapePods = value;
            }
        }

        [ProtoMember(2)]
        public Dictionary<NitroxId, EscapePodModel> SerializedEscapePodsByPlayerId
        {
            get
            {
                lock (EscapePodsByPlayerId)
                {
                    return new Dictionary<NitroxId, EscapePodModel>(EscapePodsByPlayerId);
                }
            }
            set
            {
                EscapePodsByPlayerId = value;
            }
        }

        [ProtoMember(3)]
        public EscapePodModel PodNotFullYet;

        [ProtoIgnore]
        public List<EscapePodModel> EscapePods = new List<EscapePodModel>();

        [ProtoIgnore]
        public Dictionary<NitroxId, EscapePodModel> EscapePodsByPlayerId = new Dictionary<NitroxId, EscapePodModel>();
    }
}
