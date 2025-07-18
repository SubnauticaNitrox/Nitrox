namespace Nitrox.Launcher.Models.Design;

public record OutputLine
{
    public required string Timestamp { get; init; }
    public required string LogText { get; init;  }
    public OutputLineType Type { get; init; }
}
