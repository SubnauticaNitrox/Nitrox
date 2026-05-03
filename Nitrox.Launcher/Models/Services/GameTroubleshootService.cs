using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Services;

internal sealed class GameTroubleshootService
{
    public async Task<GameCrashCause> TryDetectGameCrashAsync(TimeSpan timeout)
    {
        using CancellationTokenSource cts = new(timeout);
        return await TryDetectGameCrashAsync(cts.Token);
    }

    public async Task<GameCrashCause> TryDetectGameCrashAsync(CancellationToken cancellationToken)
    {
        // Only Windows, we always skip Discord SDK on other platforms.
        if (!OperatingSystem.IsWindows())
        {
            return GameCrashCause.NO_CRASH;
        }

        try
        {
            bool skipWait = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (ProcessEx.ProcessExists(GameInfo.Subnautica.ExeName))
                    {
                        continue;
                    }
                    if (await HasDiscordSdkCrashInLogsAsync())
                    {
                        skipWait = true;
                        return GameCrashCause.DISCORD_SDK;
                    }
                }
                finally
                {
                    if (!skipWait)
                    {
                        await Task.Delay(2000, cancellationToken);
                    }
                }
            }
            return GameCrashCause.NO_CRASH;
        }
        catch (OperationCanceledException)
        {
            return GameCrashCause.NO_CRASH;
        }
    }

    private async Task<bool> HasDiscordSdkCrashInLogsAsync()
    {
        try
        {
            string subnauticaAppDataPath = GetSubnauticaAppDataPath();
            if (subnauticaAppDataPath == "")
            {
                return false;
            }

            string content = await File.ReadAllTextAsync(Path.Combine(subnauticaAppDataPath, "Player.log"));
            if (content.Contains("Crash!!!", StringComparison.Ordinal) && content.Contains("(discord_game_sdk)", StringComparison.Ordinal))
            {
                return true;
            }
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private string GetSubnauticaAppDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Unknown Worlds", GameInfo.Subnautica.Name);

    public enum GameCrashCause
    {
        NO_CRASH,
        DISCORD_SDK
    }
}
