using System.Runtime.Versioning;
using Nitrox.Model.Platforms.OS.MacOS;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Model.Platforms.OS.MacOS;

[TestClass]
[SupportedOSPlatform("macos")]
public class MacFileSystemTest
{
    [OSTestMethod(OperatingSystems.OSX)]
    public void SetFullAccessToCurrentUser_ShouldMakeReadOnlyDirectoryWritable()
    {
        MacFileSystem fileSystem = new();
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserExecute);

        bool result = fileSystem.SetFullAccessToCurrentUser(tempDir);

        result.Should().BeTrue();
        fileSystem.IsWritable(tempDir).Should().BeTrue();

        Directory.Delete(tempDir, true);
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void SetFullAccessToCurrentUser_ShouldReturnFalseForNonexistentDirectory()
    {
        MacFileSystem fileSystem = new();
        string fakePath = Path.Combine(Path.GetTempPath(), $"NitroxTest_nonexistent_{Guid.NewGuid():N}");

        bool result = fileSystem.SetFullAccessToCurrentUser(fakePath);

        result.Should().BeFalse();
    }
}
