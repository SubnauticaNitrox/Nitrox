using System;

namespace Nitrox.Launcher.Models.Design;

public record OutputLine
{
    public required DateTimeOffset? LocalTime { get; init; }
    public required string LogText { get; init; }
    public OutputLineType Type { get; init; }
    public string LocalTimeText => LocalTime == null ? "" : $"[{LocalTime:hh:mm:ss.fff}] ";
}
