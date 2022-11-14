using System;
using Newtonsoft.Json;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class SaveFileVersion
    {
        [JsonProperty, ProtoMember(1)]
        public readonly int Major;

        [JsonProperty, ProtoMember(2)]
        public readonly int Minor;

        [JsonProperty, ProtoMember(3)]
        public readonly int Build;

        [JsonProperty, ProtoMember(4)]
        public readonly int Revision;

        public Version Version => new Version(Major, Minor, Build, Revision);

        public SaveFileVersion()
        {
            Major = NitroxEnvironment.Version.Major;
            Minor = NitroxEnvironment.Version.Minor;
            Build = NitroxEnvironment.Version.Build;
            Revision = NitroxEnvironment.Version.Revision;
        }

        public SaveFileVersion(Version version)
        {
            Major = version.Major;
            Minor = version.Minor;
            Build = version.Build;
            Revision = version.Revision;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }
    }
}
