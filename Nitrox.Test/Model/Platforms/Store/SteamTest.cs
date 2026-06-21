using System.Diagnostics;
using Nitrox.Model.Helper;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Model.Platforms.Store;

[TestClass]
public class SteamTest
{
    [OSTestMethod(OperatingSystems.Linux)]
    public void CreateSteamGameStartInfo_ShouldUseInstalledProtonFallbackAndExposeLauncherPath()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxSteamTest_{Guid.NewGuid():N}");
        try
        {
            string steamPath = Path.Combine(tempDir, "Steam");
            string gamePath = Path.Combine(steamPath, "steamapps", "common", "Subnautica", "Subnautica.exe");
            string steamExe = Path.Combine(steamPath, "steam");
            string proton10Path = Path.Combine(steamPath, "steamapps", "common", "Proton 10.0", "proton");
            string protonExperimentalPath = Path.Combine(steamPath, "steamapps", "common", "Proton - Experimental", "proton");
            string sniperEntryPoint = Path.Combine(steamPath, "steamapps", "common", "SteamLinuxRuntime_sniper", "_v2-entry-point");
            string launcherPath = NitroxUser.LauncherPath;

            WriteFile(steamExe, "");
            WriteFile(gamePath, "");
            WriteFile(proton10Path, "");
            WriteFile(protonExperimentalPath, "");
            WriteFile(sniperEntryPoint, "");
            WriteFile(Path.Combine(steamPath, "config", "config.vdf"), @"""InstallConfigStore"" { }");
            WriteFile(Path.Combine(steamPath, "config", "libraryfolders.vdf"), $$"""
                "libraryfolders"
                {
                    "0"
                    {
                        "path" "{{steamPath}}"
                        "apps"
                        {
                            "1628350" "1"
                            "264710" "1"
                        }
                    }
                }
                """);

            ProcessStartInfo startInfo = CreateSteamGameStartInfo(gamePath, steamExe, "", 264710, true);

            startInfo.FileName.Should().Be(sniperEntryPoint);
            startInfo.Arguments.Should().Contain(Path.Combine(Path.GetDirectoryName(proton10Path), "proton"));
            startInfo.Arguments.Should().NotContain(Path.Combine(Path.GetDirectoryName(protonExperimentalPath), "proton"));
            startInfo.EnvironmentVariables["PRESSURE_VESSEL_FILESYSTEMS_RW"].Should().Contain(launcherPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private static ProcessStartInfo CreateSteamGameStartInfo(string gameFilePath, string steamExe, string args, int steamAppId, bool skipSteam)
    {
        MethodInfo method = typeof(Steam).GetMethod("CreateSteamGameStartInfo", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();
        return (ProcessStartInfo)method.Invoke(null, [gameFilePath, steamExe, args, steamAppId, skipSteam, false]);
    }

    private static void WriteFile(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, contents);
    }
}
