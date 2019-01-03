using System;
using System.Collections.Generic;
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
                lock (escapePods)
                {
                    return escapePods;
                }
            }
            set
            {
                escapePods = value;
            }
        }

        [ProtoMember(2)]
        public Dictionary<ushort, EscapePodModel> SerializedEscapePodsByPlayerId
        {
            get
            {
                lock (escapePodsByPlayerId)
                {
                    return new Dictionary<ushort, EscapePodModel>(escapePodsByPlayerId);
                }
            }
            set
            {
                escapePodsByPlayerId = value;
            }
        }

        [ProtoMember(3)]
        public EscapePodModel podNotFullYet;

        [ProtoIgnore]
        public List<EscapePodModel> escapePods = new List<EscapePodModel>();

        [ProtoIgnore]
        public Dictionary<ushort, EscapePodModel> escapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();
    }
}
