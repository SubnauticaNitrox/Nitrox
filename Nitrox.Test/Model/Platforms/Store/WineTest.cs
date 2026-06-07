using Nitrox.Model;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Store;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Test.Model.Platforms.Store;

[TestClass]
public class WineTest
{
    [OSTestMethod(OperatingSystems.OSX)]
    public void InferWinePrefix_ShouldReturnPrefixContainingDriveC()
    {
        string tempDir = CreateTempDir();
        try
        {
            string prefix = Path.Combine(tempDir, "Games", "SubnauticaPrefix");
            string exePath = Path.Combine(prefix, "drive_c", "Program Files (x86)", "Steam", "steamapps", "common", "Subnautica", "Subnautica.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(exePath)!);
            File.WriteAllText(exePath, "");

            Wine.InferWinePrefix(exePath).Should().Be(prefix);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void CreateStartInfo_ShouldBuildWineLaunchCommand()
    {
        string tempDir = CreateTempDir();
        try
        {
            string prefix = Path.Combine(tempDir, ".wine");
            string gameRoot = Path.Combine(prefix, "drive_c", "Program Files (x86)", "Steam", "steamapps", "common", "Subnautica");
            string exePath = Path.Combine(gameRoot, "Subnautica.exe");
            string winePath = Path.Combine(tempDir, "bin", "wine64");
            string launcherPath = Path.Combine(tempDir, "Nitrox Launcher.app", "Contents", "MacOS", "Nitrox.Launcher");
            Directory.CreateDirectory(gameRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(winePath)!);
            File.WriteAllText(exePath, "");
            File.WriteAllText(winePath, "");

            System.Diagnostics.ProcessStartInfo startInfo = Wine.CreateStartInfo(exePath, "-vrmode none --nitrox \"launcher with spaces\"", winePath, launcherPath);

            startInfo.FileName.Should().Be(winePath);
            startInfo.WorkingDirectory.Should().Be(gameRoot);
            startInfo.ArgumentList.Should().Equal(exePath, "-vrmode", "none", "--nitrox", "launcher with spaces");
            startInfo.EnvironmentVariables[NitroxUser.LAUNCHER_PATH_ENV_KEY].Should().Be(launcherPath);
            startInfo.EnvironmentVariables["WINEPREFIX"].Should().Be(prefix);
            startInfo.UseShellExecute.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [TestMethod]
    public void SplitArguments_ShouldPreserveQuotedValues()
    {
        Wine.ParseArguments("-vrmode none --nitrox \"path with spaces\" --flag").Should()
            .Equal("-vrmode", "none", "--nitrox", "path with spaces", "--flag");
    }

    private static string CreateTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxWineTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}
