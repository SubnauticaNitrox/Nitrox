using System;
using System.Runtime.Serialization;

namespace NitroxServer.Serialization.World;

[DataContract]
public class SaveFileVersion
{
    [DataMember(Order = 1)]
    public readonly int Major;

    [DataMember(Order = 2)]
    public readonly int Minor;

    [DataMember(Order = 3)]
    public readonly int Build;

    [DataMember(Order = 4)]
    public readonly int Revision;

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

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
