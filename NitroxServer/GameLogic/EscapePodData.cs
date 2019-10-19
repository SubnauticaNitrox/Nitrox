using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodData
    {
        [ProtoIgnore]
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

        [ProtoIgnore]
        public Dictionary<ushort, EscapePodModel> SerializedEscapePodsByPlayerId
        {
            get
            {
                lock (EscapePodsByPlayerId)
                {
                    return new Dictionary<ushort, EscapePodModel>(EscapePodsByPlayerId);
                }
            }
            set
            {
                EscapePodsByPlayerId = value;
            }
        }

        [ProtoMember(3)]
        public EscapePodModel PodNotFullYet;

        [ProtoMember(1)]
        public List<EscapePodModel> EscapePods = new List<EscapePodModel>();

        [ProtoMember(2)]
        public Dictionary<ushort, EscapePodModel> EscapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();

        static EscapePodData()
        {
            Serializer.PrepareSerializer<EscapePodData>();
            Serializer.PrepareSerializer<Dictionary<ushort, EscapePodModel>>();
            Serializer.PrepareSerializer<List<EscapePodModel>>();
            Serializer.PrepareSerializer<EscapePodModel>();
        }
    }
}
