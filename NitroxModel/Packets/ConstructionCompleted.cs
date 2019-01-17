using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : Packet
    {
        public string Guid { get; }
        public string BaseGuid { get; }

        public ConstructionCompleted(string guid, string baseGuid)
        {
            Guid = guid;
            BaseGuid = baseGuid;
        }

        public override string ToString()
        {
            return "[ConstructionCompleted Guid: " + Guid + " BaseGuid: " + BaseGuid + "]";
        }
    }
}
