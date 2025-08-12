namespace Nitrox.Launcher.Models.Design;

public record OutputLine
{
    public string Timestamp { get; init; }
    public string LogText { get; init;  }
    public OutputLineType Type { get; init; }
}
