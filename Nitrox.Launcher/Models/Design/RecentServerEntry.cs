using System;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;

namespace Nitrox.Launcher.Models.Design;

public partial class RecentServerEntry : ObservableObject
{
    public static Bitmap DefaultServerIcon { get; } = AssetHelper.GetAssetFromStream("/Assets/Images/subnautica-icon.png", static stream => new Bitmap(stream));

    [ObservableProperty]
    public partial string LocalServerName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServerInfoTooltip))]
    public partial string? RemoteHostServerName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServerInfoTooltip))]
    [NotifyPropertyChangedFor(nameof(VersionMismatchTooltip))]
    public partial string? NitroxVersion { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VersionMismatchTooltip))]
    [NotifyPropertyChangedFor(nameof(ShowVersionMismatchWarning))]
    public partial bool IsVersionCompatible { get; set; } = true;

    public string ServerIP { get; set; } = string.Empty;

    public int ServerPort { get; set; } = SubnauticaServerConstants.DEFAULT_PORT;

    [ObservableProperty]
    public partial Bitmap ServerIcon { get; set; } = DefaultServerIcon;

    [ObservableProperty]
    public partial bool IsOnline { get; set; }

    [ObservableProperty]
    public partial bool IsStatusLoading { get; set; }

    [ObservableProperty]
    public partial int PlayerCount { get; set; }

    [ObservableProperty]
    public partial int MaxPlayers { get; set; } = SubnauticaServerConstants.DEFAULT_MAX_PLAYERS;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayerNamesTooltip))]
    public partial AvaloniaList<string> PlayerNames { get; set; } = [];

    public string? PlayerNamesTooltip => PlayerNames.Count == 0 ? null : string.Join(Environment.NewLine, PlayerNames);

    public string? ServerInfoTooltip
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ServerIP))
            {
                return null;
            }

            string endpoint = $"IP: {ServerIP}{Environment.NewLine}Port: {ServerPort}";

            return string.IsNullOrWhiteSpace(RemoteHostServerName) ? endpoint : $"Host server name: {RemoteHostServerName}{Environment.NewLine}{endpoint}";
        }
    }

    public string? VersionMismatchTooltip => $"Server version: {NitroxVersion}{Environment.NewLine}Your version: {NitroxEnvironment.Version}";


    public bool ShowVersionMismatchWarning => !IsStatusLoading && !IsVersionCompatible;
}
