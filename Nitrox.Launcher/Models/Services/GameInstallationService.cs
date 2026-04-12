using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.Store;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Launcher.Models.Services;

internal sealed partial class GameInstallationService : ObservableObject
{
    private readonly AvaloniaList<KnownGame> installedGames = [];

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; } = new() { PathToGame = string.Empty, Platform = Platform.NONE };

    public AvaloniaList<KnownGame> InstalledGames => installedGames;

    public IReadOnlyList<KnownGame> RefreshInstalledGames(GameInfo gameInfo)
    {
        List<KnownGame> discoveredGames = GameInstallationFinder.FindGamesCached(gameInfo)
            .Select(ToKnownGame)
            .DistinctBy(game => game.PathToGame, StringComparer.OrdinalIgnoreCase)
            .ToList();

        KnownGame? selectedGame = TryGetSelectedGame(gameInfo, discoveredGames) ?? discoveredGames.FirstOrDefault();

        installedGames.Clear();
        foreach (KnownGame discoveredGame in discoveredGames)
        {
            installedGames.Add(discoveredGame);
        }

        if (selectedGame is null)
        {
            SelectedGame = new KnownGame { PathToGame = string.Empty, Platform = Platform.NONE };
            return discoveredGames;
        }

        PromoteInstalledGame(selectedGame);
        ApplySelection(selectedGame);
        return discoveredGames;
    }

    public void SelectGameInstallation(KnownGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        KnownGame normalizedGame = Normalize(game);
        PromoteInstalledGame(normalizedGame);
        ApplySelection(normalizedGame);
    }

    public void SelectGameInstallation(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        SelectGameInstallation(new KnownGame
        {
            PathToGame = Path.GetFullPath(path),
            Platform = GetPlatformFromPath(path)
        });
    }

    private static KnownGame? TryGetSelectedGame(GameInfo gameInfo, IEnumerable<KnownGame> discoveredGames)
    {
        string selectedPath = NitroxUser.GamePath;
        if (!string.IsNullOrWhiteSpace(selectedPath) && GameInstallationHelper.HasValidGameFolder(selectedPath, gameInfo))
        {
            return new KnownGame
            {
                PathToGame = Path.GetFullPath(selectedPath),
                Platform = NitroxUser.GamePlatform?.Platform ?? GetPlatformFromPath(selectedPath)
            };
        }

        string preferredPath = NitroxUser.PreferredGamePath;
        if (!string.IsNullOrWhiteSpace(preferredPath) && GameInstallationHelper.HasValidGameFolder(preferredPath, gameInfo))
        {
            return new KnownGame
            {
                PathToGame = Path.GetFullPath(preferredPath),
                Platform = GetPlatformFromPath(preferredPath)
            };
        }

        return discoveredGames.FirstOrDefault();
    }

    private static KnownGame Normalize(KnownGame game)
    {
        string path = Path.GetFullPath(game.PathToGame);
        Platform platform = game.Platform == Platform.NONE ? GetPlatformFromPath(path) : game.Platform;
        return new KnownGame
        {
            PathToGame = path,
            Platform = platform
        };
    }

    private void PromoteInstalledGame(KnownGame game)
    {
        for (int i = installedGames.Count - 1; i >= 0; i--)
        {
            if (string.Equals(installedGames[i].PathToGame, game.PathToGame, StringComparison.OrdinalIgnoreCase))
            {
                installedGames.RemoveAt(i);
            }
        }

        installedGames.Insert(0, game);
    }

    private void ApplySelection(KnownGame game)
    {
        SelectedGame = game;
        NitroxUser.PreferredGamePath = game.PathToGame;
        NitroxUser.SetGamePathAndPlatform(game.PathToGame, ResolveGamePlatform(game));
    }

    private static IGamePlatform? ResolveGamePlatform(KnownGame game)
    {
        if (Enum.TryParse<GameLibraries>(game.Platform.ToString(), out GameLibraries gameLibrary))
        {
            return GamePlatforms.GetPlatformByFlag(gameLibrary);
        }

        return GamePlatforms.GetPlatformByGameDir(game.PathToGame);
    }

    private static Platform GetPlatformFromPath(string path) => GamePlatforms.GetPlatformByGameDir(path)?.Platform ?? Platform.NONE;

    private static KnownGame ToKnownGame(GameFinderResult gameFinderResult)
    {
        Platform platform = GamePlatforms.GetPlatformByFlag(gameFinderResult.Origin)?.Platform ?? GamePlatforms.GetPlatformByGameDir(gameFinderResult.Path)?.Platform ?? Platform.NONE;
        return new KnownGame
        {
            PathToGame = gameFinderResult.Path,
            Platform = platform
        };
    }
}
