using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class StoryTimingData
{
    /// <summary>
    ///     Game time elapsed in seconds.
    /// </summary>
    public double ElapsedSeconds { get; set; }

    public double? AuroraCountdownTime { get; set; }

    public double? AuroraWarningTime { get; set; }

    /// <summary>
    ///     Time elapsed in real-time. In seconds.
    /// </summary>
    public double RealTimeElapsed { get; set; }

    public double? AuroraRealExplosionTime { get; set; }

    public static StoryTimingData From(StoryManager storyManager, TimeService timeService)
    {
        return new StoryTimingData
        {
            ElapsedSeconds = timeService.GameTime.TotalSeconds,
            AuroraCountdownTime = storyManager.AuroraCountdownTimeMs,
            AuroraWarningTime = storyManager.AuroraWarningTimeMs,
            RealTimeElapsed = timeService.ActiveTime.TotalSeconds,
            AuroraRealExplosionTime = storyManager.AuroraRealExplosionTime.TotalSeconds
        };
    }
}
