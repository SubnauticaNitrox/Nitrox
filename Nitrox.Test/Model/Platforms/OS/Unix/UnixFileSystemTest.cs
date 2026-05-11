using System.Runtime.Versioning;
using Nitrox.Model.Platforms.OS.Unix;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Model.Platforms.OS.Unix;

[TestClass]
[SupportedOSPlatform("linux")]
public class UnixFileSystemTest
{
    [OSTestMethod(OperatingSystems.Linux)]
    public void SetFullAccessToCurrentUser_ShouldMakeReadOnlyDirectoryWritable()
    {
        UnixFileSystem fileSystem = new();
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserExecute);

        bool result = fileSystem.SetFullAccessToCurrentUser(tempDir);

        result.Should().BeTrue();
        fileSystem.IsWritable(tempDir).Should().BeTrue();

        Directory.Delete(tempDir, true);
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void SetFullAccessToCurrentUser_ShouldReturnFalseForNonexistentDirectory()
    {
        UnixFileSystem fileSystem = new();
        string fakePath = Path.Combine(Path.GetTempPath(), $"NitroxTest_nonexistent_{Guid.NewGuid():N}");

        bool result = fileSystem.SetFullAccessToCurrentUser(fakePath);

        result.Should().BeFalse();
    }
}
