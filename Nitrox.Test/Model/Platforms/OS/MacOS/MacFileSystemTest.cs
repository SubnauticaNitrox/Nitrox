using System.Runtime.Versioning;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Model.Platforms.OS.MacOS;

[TestClass]
[SupportedOSPlatform("macos")]
public class MacFileSystemTest
{
    [OSTestMethod(OperatingSystems.OSX)]
    public void SetFullAccessToCurrentUser_ShouldMakeReadOnlyDirectoryWritable()
    {
        // Arrange
        MacFileSystem fileSystem = new();
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxTest_{Guid.NewGuid():N}");

        // Act
        Directory.CreateDirectory(tempDir);
        File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        bool result = fileSystem.SetFullAccessToCurrentUser(tempDir);

        // Assert
        result.Should().BeTrue();
        fileSystem.IsWritable(tempDir).Should().BeTrue();

        Directory.Delete(tempDir, true);
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void SetFullAccessToCurrentUser_ShouldReturnFalseForNonexistentDirectory()
    {
        // Arrange
        MacFileSystem fileSystem = new();
        string fakePath = Path.Combine(Path.GetTempPath(), $"NitroxTest_nonexistent_{Guid.NewGuid():N}");

        // Act
        bool result = fileSystem.SetFullAccessToCurrentUser(fakePath);

        // Assert
        result.Should().BeFalse();
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void SetFullAccessToCurrentUser_ShouldReturnFalseForRootDirectory()
    {
        // Arrange
        MacFileSystem fileSystem = new();

        // Act
        bool result = fileSystem.SetFullAccessToCurrentUser("/");

        // Assert
        result.Should().BeFalse();
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void IsRootDirectory_SanityCheck()
    {
        // Arrange
        MacFileSystem fileSystem = new();

        // Act & Assert
        fileSystem.IsRootDirectory(null!).Should().BeFalse();
        fileSystem.IsRootDirectory("").Should().BeFalse();
        fileSystem.IsRootDirectory(" ").Should().BeFalse();
        fileSystem.IsRootDirectory("/").Should().BeTrue();
        fileSystem.IsRootDirectory("//").Should().BeTrue();
        fileSystem.IsRootDirectory("///").Should().BeTrue();
        fileSystem.IsRootDirectory("/tmp/").Should().BeFalse();
        fileSystem.IsRootDirectory("/Users/").Should().BeFalse();
    }
}
