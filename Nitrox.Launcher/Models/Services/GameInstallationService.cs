using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.Discovery.InstallationFinders.Core;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Launcher.Models.Services;

internal sealed partial class GameInstallationService : ObservableObject
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private bool hasLoggedInitialCacheSnapshot;
    private readonly HashSet<string> ignoredGamePaths = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; } = new() { PathToGame = string.Empty, Platform = Platform.NONE };

    public AvaloniaList<KnownGame> InstalledGames { get; } = [];

    public async Task<List<KnownGame>> RefreshInstalledGamesAsync(GameInfo gameInfo)
    {
        GameInstallationCacheData cacheData = await LoadKnownGamesAsync(gameInfo);
        await LogInitialCacheSnapshotAsync(gameInfo, cacheData);
        ReplaceIgnoredGamePaths(cacheData.IgnoredGamePaths);

        List<KnownGame> savedGames = await FilterValidSavedGamesAsync(gameInfo, cacheData.KnownGames);
        List<KnownGame> discoveredGames = GameInstallationFinder
                                          .FindGamesCached(gameInfo)
                                          .Select(ToKnownGame)
                                          .Where(game => !IsIgnoredGamePath(game.PathToGame))
                                          .ToList();

        List<KnownGame> mergedGames = savedGames
                                      .Concat(discoveredGames)
                                      .Where(game => !string.IsNullOrWhiteSpace(game.PathToGame))
                                      .Select(Normalize)
                                      .DistinctBy(game => game.PathToGame, StringComparer.OrdinalIgnoreCase)
                                      .ToList();

        SelectGameAndUpdateUI(gameInfo, mergedGames);
        await SaveKnownGamesAsync(gameInfo);
        return mergedGames;
    }

    public bool AddGameInstallation(GameInfo gameInfo, string path, out string? errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string normalizedPath = Path.GetFullPath(path);
        if (!GameInstallationHelper.HasValidGameFolder(normalizedPath, gameInfo))
        {
            errorMessage = $"Invalid {gameInfo.Name} directory";
            return false;
        }

        PirateDetection.TriggerOnDirectory(normalizedPath);
        if (!FileSystem.Instance.IsWritable(Directory.GetCurrentDirectory()) || !FileSystem.Instance.IsWritable(normalizedPath))
        {
            if (!FileSystem.Instance.SetFullAccessToCurrentUser(Directory.GetCurrentDirectory()) || !FileSystem.Instance.SetFullAccessToCurrentUser(normalizedPath))
            {
                errorMessage = "Restart Nitrox Launcher as admin to allow Nitrox to change permissions as needed. This is only needed once. Nitrox will close after this message.";
                return false;
            }
        }

        SelectGameInstallation(gameInfo, normalizedPath);
        errorMessage = null;
        return true;
    }

    public void SelectGameInstallation(GameInfo gameInfo, KnownGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        SelectGameInstallation(gameInfo, game, promoteInstalledGame: false);
    }

    public bool RemoveGameInstallation(GameInfo gameInfo, KnownGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        KnownGame normalizedGame = Normalize(game);
        bool removed = false;
        for (int i = InstalledGames.Count - 1; i >= 0; i--)
        {
            if (string.Equals(InstalledGames[i].PathToGame, normalizedGame.PathToGame, StringComparison.OrdinalIgnoreCase))
            {
                InstalledGames.RemoveAt(i);
                removed = true;
            }
        }

        AddIgnoredGamePath(normalizedGame.PathToGame);

        if (IsGamePathSelected(normalizedGame.PathToGame))
        {
            KnownGame? fallbackGame = InstalledGames.FirstOrDefault();
            if (fallbackGame is null)
            {
                SelectedGame = new KnownGame { PathToGame = string.Empty, Platform = Platform.NONE };
                NitroxUser.PreferredGamePath = string.Empty;
                NitroxUser.ClearGamePathAndPlatform();
            }
            else
            {
                ApplySelection(fallbackGame);
            }
        }

        _ = Task.Run(() => SaveKnownGamesAsync(gameInfo));
        return removed;
    }

    private void SelectGameInstallation(GameInfo gameInfo, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        SelectGameInstallation(gameInfo, new KnownGame
        {
            PathToGame = Path.GetFullPath(path),
            Platform = GetPlatformFromPath(path)
        }, promoteInstalledGame: true);
    }

    private void SelectGameInstallation(GameInfo gameInfo, KnownGame game, bool promoteInstalledGame)
    {
        KnownGame normalizedGame = Normalize(game);
        RemoveIgnoredGamePath(normalizedGame.PathToGame);
        if (promoteInstalledGame)
        {
            AddOrUpdateInstalledGame(normalizedGame);
        }
        ApplySelection(normalizedGame);
        _ = Task.Run(() => SaveKnownGamesAsync(gameInfo));
    }

    private void AddOrUpdateInstalledGame(KnownGame game)
    {
        for (int i = 0; i < InstalledGames.Count; i++)
        {
            if (string.Equals(InstalledGames[i].PathToGame, game.PathToGame, StringComparison.OrdinalIgnoreCase))
            {
                InstalledGames[i] = game;
                return;
            }
        }

        InstalledGames.Add(game);
    }

    private async Task LogInitialCacheSnapshotAsync(GameInfo gameInfo, GameInstallationCacheData cacheData)
    {
        if (hasLoggedInitialCacheSnapshot)
        {
            return;
        }

        await Task.Run(() =>
        {
            string cachedInstallations = cacheData.KnownGames.Count == 0
                ? "    none"
                : string.Join(Environment.NewLine, cacheData.KnownGames.Select(game => $"    {game.PathToGame}"));

            string ignoredInstallations = cacheData.IgnoredGamePaths.Count == 0
                ? "    none"
                : string.Join(Environment.NewLine, cacheData.IgnoredGamePaths
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => $"    {path}"));

            Log.Info($"Loaded cached {gameInfo.Name} installations:{Environment.NewLine}{cachedInstallations}");
            Log.Info($"Ignored {gameInfo.Name} installations:{Environment.NewLine}{ignoredInstallations}");
        });

        hasLoggedInitialCacheSnapshot = true;
    }

    private async Task<List<KnownGame>> FilterValidSavedGamesAsync(GameInfo gameInfo, IEnumerable<KnownGame> savedGamesFromCache)
    {
        List<KnownGame> validGames = [];

        foreach (KnownGame normalizedGame in savedGamesFromCache
            .Select(Normalize)
            .Where(game => !string.IsNullOrWhiteSpace(game.PathToGame)))
        {
            bool exists = await Task.Run(() => Directory.Exists(normalizedGame.PathToGame));
            if (!exists)
            {
                Log.Info($"Removing missing {gameInfo.Name} installation from cache: {normalizedGame.PathToGame}");
                continue;
            }

            if (!IsIgnoredGamePath(normalizedGame.PathToGame))
            {
                validGames.Add(normalizedGame);
            }
        }

        return validGames;
    }

    private void SelectGameAndUpdateUI(GameInfo gameInfo, List<KnownGame> mergedGames)
    {
        KnownGame? selectedGame = TryGetSelectedGame(gameInfo, mergedGames);
        selectedGame ??= mergedGames.FirstOrDefault();

        InstalledGames.Clear();
        foreach (KnownGame game in mergedGames)
        {
            InstalledGames.Add(game);
        }

        if (selectedGame is null)
        {
            SelectedGame = new KnownGame { PathToGame = string.Empty, Platform = Platform.NONE };
            NitroxUser.PreferredGamePath = string.Empty;
            NitroxUser.ClearGamePathAndPlatform();
        }
        else
        {
            ApplySelection(selectedGame);
        }
    }

    private async Task<GameInstallationCacheData> LoadKnownGamesAsync(GameInfo gameInfo)
    {
        try
        {
            string cacheFilePath = GetCacheFilePath(gameInfo);
            bool fileExists = await Task.Run(() => File.Exists(cacheFilePath));
            if (!fileExists)
            {
                ignoredGamePaths.Clear();
                return new GameInstallationCacheData();
            }

            string serialized = await File.ReadAllTextAsync(cacheFilePath);
            GameInstallationCacheData? cacheData = JsonSerializer.Deserialize<GameInstallationCacheData>(serialized, jsonSerializerOptions);
            if (cacheData is not null)
            {
                return NormalizeCacheData(cacheData);
            }

            List<KnownGame>? knownGames = JsonSerializer.Deserialize<List<KnownGame>>(serialized, jsonSerializerOptions);
            ignoredGamePaths.Clear();
            return new GameInstallationCacheData
            {
                KnownGames = knownGames?.Select(Normalize).Where(game => !string.IsNullOrWhiteSpace(game.PathToGame)).ToList() ?? []
            };
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to parse known game installations from cache for '{gameInfo.Name}': {ex.Message}");
            ignoredGamePaths.Clear();
            return new GameInstallationCacheData();
        }
    }

    private async Task SaveKnownGamesAsync(GameInfo gameInfo)
    {
        try
        {
            await Task.Run(() => Directory.CreateDirectory(NitroxUser.CachePath));
            GameInstallationCacheData cacheData = new()
            {
                KnownGames = InstalledGames.Select(Normalize).Where(game => !string.IsNullOrWhiteSpace(game.PathToGame)).ToList(),
                IgnoredGamePaths = ignoredGamePaths.ToList()
            };
            string serialized = JsonSerializer.Serialize(cacheData, jsonSerializerOptions);
            await File.WriteAllTextAsync(GetCacheFilePath(gameInfo), serialized);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to save known game installations for '{gameInfo.Name}'");
        }
    }

    private static string GetCacheFilePath(GameInfo gameInfo)
    {
        string cacheKey = $"game-installations-{gameInfo.Name.ToLowerInvariant()}";
        string hash = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(cacheKey)));
        return Path.Combine(NitroxUser.CachePath, $"nitrox_gi_{hash}.cache");
    }

    private KnownGame? TryGetSelectedGame(GameInfo gameInfo, List<KnownGame> discoveredGames)
    {
        string? selectedPath = GetValidGamePath(NitroxUser.GamePath, gameInfo);
        if (selectedPath is not null)
        {
            return new KnownGame
            {
                PathToGame = selectedPath,
                Platform = NitroxUser.GamePlatform?.Platform ?? GetPlatformFromPath(selectedPath)
            };
        }

        string? preferredPath = GetValidGamePath(NitroxUser.PreferredGamePath, gameInfo);
        if (preferredPath is not null)
        {
            return new KnownGame
            {
                PathToGame = preferredPath,
                Platform = GetPlatformFromPath(preferredPath)
            };
        }

        return discoveredGames.FirstOrDefault();
    }

    private string? GetValidGamePath(string? path, GameInfo gameInfo)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo) || IsIgnoredGamePath(path))
        {
            return null;
        }

        return Path.GetFullPath(path);
    }

    private static KnownGame Normalize(KnownGame game)
    {
        string path = NormalizePath(game.PathToGame);
        Platform platform = game.Platform == Platform.NONE ? GetPlatformFromPath(path) : game.Platform;
        return new KnownGame
        {
            PathToGame = path,
            Platform = platform
        };
    }


    private void ApplySelection(KnownGame game)
    {
        SelectedGame = game;
        NitroxUser.PreferredGamePath = game.PathToGame;
        NitroxUser.SetGamePathAndPlatform(game.PathToGame, ResolveGamePlatform(game));
    }

    private bool IsGamePathSelected(string path)
    {
        return string.Equals(SelectedGame.PathToGame, path, StringComparison.OrdinalIgnoreCase)
            || string.Equals(NitroxUser.GamePath, path, StringComparison.OrdinalIgnoreCase)
            || string.Equals(NitroxUser.PreferredGamePath, path, StringComparison.OrdinalIgnoreCase);
    }

    private void AddIgnoredGamePath(string path)
    {
        string normalizedPath = NormalizePath(path);
        if (!string.IsNullOrWhiteSpace(normalizedPath))
        {
            ignoredGamePaths.Add(normalizedPath);
        }
    }

    private void RemoveIgnoredGamePath(string path)
    {
        string normalizedPath = NormalizePath(path);
        if (!string.IsNullOrWhiteSpace(normalizedPath))
        {
            ignoredGamePaths.Remove(normalizedPath);
        }
    }

    private bool IsIgnoredGamePath(string path)
    {
        string normalizedPath = NormalizePath(path);
        return !string.IsNullOrWhiteSpace(normalizedPath) && ignoredGamePaths.Contains(normalizedPath);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private void ReplaceIgnoredGamePaths(IEnumerable<string> paths)
    {
        ignoredGamePaths.Clear();
        foreach (string path in paths)
        {
            AddIgnoredGamePath(path);
        }
    }

    private GameInstallationCacheData NormalizeCacheData(GameInstallationCacheData cacheData)
    {
        cacheData.KnownGames = cacheData.KnownGames.Select(Normalize).Where(game => !string.IsNullOrWhiteSpace(game.PathToGame)).ToList();
        cacheData.IgnoredGamePaths = cacheData.IgnoredGamePaths
            .Select(NormalizePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        ReplaceIgnoredGamePaths(cacheData.IgnoredGamePaths);
        return cacheData;
    }

    private static IGamePlatform? ResolveGamePlatform(KnownGame game)
    {
        if (Enum.TryParse(game.Platform.ToString(), out GameLibraries gameLibrary))
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

    private sealed class GameInstallationCacheData
    {
        public List<KnownGame> KnownGames { get; set; } = [];

        public List<string> IgnoredGamePaths { get; set; } = [];
    }
}
