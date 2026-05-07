namespace Nitrox.Model.Networking;

public sealed class RemoteServerStatusResponse
{
    public bool IsOnline { get; set; } = true;

    public string NitroxVersion { get; set; } = string.Empty;

    public string ServerName { get; set; } = string.Empty;

    public string? HostServerName { get; set; }

    public int PlayerCount { get; set; }

    public string[] PlayerNames { get; set; } = [];

    public int MaxPlayerCount { get; set; }
}
