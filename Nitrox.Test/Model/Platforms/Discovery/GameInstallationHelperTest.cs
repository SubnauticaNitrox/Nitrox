using Nitrox.Model;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Test.Model.Platforms.Discovery;

[TestClass]
public class GameInstallationHelperTest
{
    [OSTestMethod(OperatingSystems.OSX)]
    public void NormalizeGamePath_ShouldAcceptMacOSSteamGameFolder()
    {
        string tempDir = CreateTempDir();
        try
        {
            string steamGameRoot = Path.Combine(tempDir, "steamapps", "common", "Subnautica");
            string appContents = Path.Combine(steamGameRoot, "Subnautica.app", "Contents");
            CreateNativeMacSubnautica(appContents);

            string normalized = GameInstallationHelper.NormalizeGamePath(steamGameRoot, GameInfo.Subnautica);

            normalized.Should().Be(appContents);
            GameInstallationHelper.HasValidGameFolder(normalized, GameInfo.Subnautica).Should().BeTrue();
            GameInstallationHelper.IsNativeMacOSGameLayout(normalized, GameInfo.Subnautica).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void NormalizeGamePath_ShouldAcceptMacOSAppBundle()
    {
        string tempDir = CreateTempDir();
        try
        {
            string steamGameRoot = Path.Combine(tempDir, "steamapps", "common", "Subnautica");
            string appBundle = Path.Combine(steamGameRoot, "Subnautica.app");
            string appContents = Path.Combine(appBundle, "Contents");
            CreateNativeMacSubnautica(appContents);

            string normalized = GameInstallationHelper.NormalizeGamePath(appBundle, GameInfo.Subnautica);

            normalized.Should().Be(appContents);
            GameInstallationHelper.TryGetGameExecutablePath(normalized, GameInfo.Subnautica, out string executablePath).Should().BeTrue();
            executablePath.Should().Be(Path.Combine(appContents, "MacOS", "Subnautica"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string CreateTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxGameInstallationHelperTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private static void CreateNativeMacSubnautica(string contentsPath)
    {
        Directory.CreateDirectory(Path.Combine(contentsPath, "MacOS"));
        Directory.CreateDirectory(Path.Combine(contentsPath, "Resources", "Data", "Managed"));
        File.WriteAllText(Path.Combine(contentsPath, "MacOS", "Subnautica"), "");
    }
}
