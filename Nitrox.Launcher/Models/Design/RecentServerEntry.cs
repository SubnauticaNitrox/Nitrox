using System;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.Models.Utils;

namespace Nitrox.Launcher.Models.Design;

public partial class RecentServerEntry : ObservableObject
{

    public string ServerName { get; set; } = string.Empty;
    public string ServerIP { get; set; } = string.Empty;
    public int ServerPort { get; set; } = 11000;

    [ObservableProperty]
    public partial Bitmap ServerIcon { get; set; } = AssetHelper.GetAssetFromStream("/Assets/Images/subnautica-icon.png", static stream => new Bitmap(stream));

    [ObservableProperty]
    public partial bool IsOnline { get; set; } = false;

    [ObservableProperty]
    public partial int PlayerCount { get; set; } = 0;

    [ObservableProperty]
    public partial int MaxPlayers { get; set; } = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayerNamesTooltip))]
    public partial AvaloniaList<string> PlayerNames { get; set; }

    public string? PlayerNamesTooltip => PlayerNames.Count == 0 ? null : string.Join(Environment.NewLine, PlayerNames);
    public string? ServerAddressTooltip => ServerIP == string.Empty ? null : $"IP: {ServerIP}\nPort: {ServerPort}";
}
