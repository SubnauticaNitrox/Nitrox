using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class SaveVersion
    {
        [ProtoMember(1)]
        public long Version;

        public SaveVersion()
        {}

        public SaveVersion(long version)
        {
            Version = version;
        }
    }
}
