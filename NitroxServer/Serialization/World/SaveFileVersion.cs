using System;
using Newtonsoft.Json;
using NitroxModel.Helper;

namespace NitroxServer.Serialization.World;

[JsonObject(MemberSerialization.OptIn)]
public class SaveFileVersion
{
    [JsonProperty]
    public readonly int Major;

    [JsonProperty]
    public readonly int Minor;

    [JsonProperty]
    public readonly int Build;

    [JsonProperty]
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
