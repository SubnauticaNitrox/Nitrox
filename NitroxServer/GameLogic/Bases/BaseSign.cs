using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    public class BaseSign
    {
        [ProtoMember(1)]
        public Dictionary<string, SignData> SerializableBaseSignByGuid
        {
            get
            {
                lock (basesignByGuid)
                {
                    return new Dictionary<string, SignData>(basesignByGuid);
                }
            }
            set { basesignByGuid = value; }
        }

        [ProtoIgnore]
        private Dictionary<string, SignData> basesignByGuid = new Dictionary<string, SignData>();

        public void UpdateBaseSign(SignData basesign)
        {
            lock(basesignByGuid)
            {
                if (basesignByGuid.ContainsKey(basesign.Guid))
                {
                    basesignByGuid[basesign.Guid] = basesign;
                }
                else
                {
                    basesignByGuid.Add(basesign.Guid, basesign);
                }
            }
        }

        public void DeleteBaseSign(string guid)
        {
            lock (basesignByGuid)
            {
                if (basesignByGuid.ContainsKey(guid))
                {
                    basesignByGuid.Remove(guid);
                }
            }
        }


        public List<SignData> GetBaseAllSign()
        {
            List<SignData> baseSign = new List<SignData>();
            lock(basesignByGuid)
            {
                foreach (SignData Sign in basesignByGuid.Values)
                {
                    baseSign.Add(Sign);
                }
            }
            return baseSign;
        }
    }
}
