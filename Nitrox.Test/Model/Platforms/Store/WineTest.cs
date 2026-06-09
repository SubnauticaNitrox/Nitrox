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

    [TestMethod]
    public void FindSteamExecutableForGame_ShouldReturnSteamExeFromSteamLibrary()
    {
        string tempDir = CreateTempDir();
        try
        {
            string steamRoot = Path.Combine(tempDir, "drive_c", "Program Files (x86)", "Steam");
            string gameRoot = Path.Combine(steamRoot, "steamapps", "common", "Subnautica");
            string steamExePath = Path.Combine(steamRoot, "steam.exe");
            string gameExePath = Path.Combine(gameRoot, "Subnautica.exe");
            Directory.CreateDirectory(gameRoot);
            File.WriteAllText(steamExePath, "");
            File.WriteAllText(gameExePath, "");

            Wine.FindSteamExecutableForGame(gameExePath).Should().Be(steamExePath);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [TestMethod]
    public void CreateSteamStartInfoIfNeeded_ShouldBuildSteamLaunchCommand()
    {
        string tempDir = CreateTempDir();
        try
        {
            string steamRoot = Path.Combine(tempDir, "drive_c", "Program Files (x86)", "Steam");
            string gameRoot = Path.Combine(steamRoot, "steamapps", "common", "Subnautica");
            string steamExePath = Path.Combine(steamRoot, "steam.exe");
            string gameExePath = Path.Combine(gameRoot, "Subnautica.exe");
            string winePath = Path.Combine(tempDir, "bin", "wine64");
            Directory.CreateDirectory(gameRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(winePath)!);
            File.WriteAllText(steamExePath, "");
            File.WriteAllText(gameExePath, "");
            File.WriteAllText(winePath, "");

            System.Diagnostics.ProcessStartInfo? startInfo = Wine.CreateSteamStartInfoIfNeeded(gameExePath, winePath);

            startInfo.Should().NotBeNull();
            startInfo!.FileName.Should().Be(winePath);
            startInfo.WorkingDirectory.Should().Be(steamRoot);
            startInfo.ArgumentList.First().Should().Be(steamExePath);
            startInfo.ArgumentList.Should().Contain("-nobootstrapupdate");
            startInfo.ArgumentList.Should().Contain("-cef-disable-gpu");
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
        string? originalDisableIpv6 = Environment.GetEnvironmentVariable("NITROX_DISABLE_IPV6");
        string? originalConnectTimeout = Environment.GetEnvironmentVariable("NITROX_CLIENT_CONNECT_TIMEOUT_MS");
        string? originalManualMode = Environment.GetEnvironmentVariable("NITROX_LITENETLIB_MANUAL_MODE");
        string? originalCrcFallback = Environment.GetEnvironmentVariable("NITROX_LITENETLIB_CRC_FALLBACK");
        string? originalDisableNativeSockets = Environment.GetEnvironmentVariable("NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS");
        try
        {
            Environment.SetEnvironmentVariable("NITROX_DISABLE_IPV6", null);
            Environment.SetEnvironmentVariable("NITROX_CLIENT_CONNECT_TIMEOUT_MS", null);
            Environment.SetEnvironmentVariable("NITROX_LITENETLIB_MANUAL_MODE", null);
            Environment.SetEnvironmentVariable("NITROX_LITENETLIB_CRC_FALLBACK", null);
            Environment.SetEnvironmentVariable("NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS", null);

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
            startInfo.EnvironmentVariables["NITROX_DISABLE_IPV6"].Should().Be("1");
            startInfo.EnvironmentVariables["NITROX_CLIENT_CONNECT_TIMEOUT_MS"].Should().Be("10000");
            startInfo.EnvironmentVariables["NITROX_LITENETLIB_MANUAL_MODE"].Should().Be("0");
            startInfo.EnvironmentVariables["NITROX_LITENETLIB_CRC_FALLBACK"].Should().Be("1");
            startInfo.EnvironmentVariables["NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS"].Should().Be("1");
            startInfo.UseShellExecute.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NITROX_DISABLE_IPV6", originalDisableIpv6);
            Environment.SetEnvironmentVariable("NITROX_CLIENT_CONNECT_TIMEOUT_MS", originalConnectTimeout);
            Environment.SetEnvironmentVariable("NITROX_LITENETLIB_MANUAL_MODE", originalManualMode);
            Environment.SetEnvironmentVariable("NITROX_LITENETLIB_CRC_FALLBACK", originalCrcFallback);
            Environment.SetEnvironmentVariable("NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS", originalDisableNativeSockets);
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
