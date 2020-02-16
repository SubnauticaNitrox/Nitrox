using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EscapePodData
    {
        [ProtoMember(1)]
        private List<EscapePodModel> SerializedEscapePods
        {
            get
            {
                lock (EscapePods)
                {
                    serializedEscapePods = new List<EscapePodModel>(EscapePods);
                    return EscapePods;
                }
            }
            set
            {
                serializedEscapePods = EscapePods = value;
            }
        }

        private List<EscapePodModel> serializedEscapePods = new List<EscapePodModel>();

        [ProtoMember(2)]
        private Dictionary<ushort, EscapePodModel> SerializedEscapePodsByPlayerId
        {
            get
            {
                lock (EscapePodsByPlayerId)
                {
                    serializedEscapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>(EscapePodsByPlayerId);
                    return serializedEscapePodsByPlayerId;
                }
            }
            set
            {
                serializedEscapePodsByPlayerId = EscapePodsByPlayerId = value;
            }
        }

        Dictionary<ushort, EscapePodModel> serializedEscapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();

        [ProtoMember(3)]
        public EscapePodModel PodNotFullYet;

        public List<EscapePodModel> EscapePods = new List<EscapePodModel>();

        public Dictionary<ushort, EscapePodModel> EscapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            lock (EscapePods)
            {
                EscapePods = serializedEscapePods;
            }

            lock (EscapePodsByPlayerId)
            {
                EscapePodsByPlayerId = serializedEscapePodsByPlayerId;
            }
        }
    }
}
