using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Nitrox.Model.Core;

namespace Nitrox.Model.Serialization;

[DataContract]
public class SaveFileVersion
{
    [DataMember(Order = 1)]
    public int Major { get; }

    [DataMember(Order = 2)]
    public int Minor { get; }

    [DataMember(Order = 3)]
    public int Build { get; }

    [DataMember(Order = 4)]
    public int Revision { get; }

    [JsonIgnore]
    public Version Version => new(Major, Minor, Build, Revision);

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

    [JsonConstructor]
    public SaveFileVersion(int major, int minor, int build, int revision)
    {
        Major = major;
        Minor = minor;
        Build = build;
        Revision = revision;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
