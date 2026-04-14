using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    private readonly HashSet<string> ignoredGamePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; } = new() { PathToGame = string.Empty, Platform = Platform.NONE };

    public AvaloniaList<KnownGame> InstalledGames { get; } = [];

    public List<KnownGame> RefreshInstalledGames(GameInfo gameInfo)
    {
        GameInstallationCacheData cacheData = LoadKnownGames(gameInfo);
        if (!hasLoggedInitialCacheSnapshot)
        {
            string cachedInstallations = cacheData.KnownGames.Count == 0
                ? "    none"
                : string.Join(Environment.NewLine, cacheData.KnownGames.Select(game => $"    {game.PathToGame}"));
            string ignoredInstallations = cacheData.IgnoredGamePaths.Count == 0
                ? "    none"
                : string.Join(Environment.NewLine, cacheData.IgnoredGamePaths.Where(path => !string.IsNullOrWhiteSpace(path)).Select(path => $"    {path}"));

            Log.Info($"Loaded cached {gameInfo.Name} installations:{Environment.NewLine}{cachedInstallations}");
            Log.Info($"Ignored {gameInfo.Name} installations:{Environment.NewLine}{ignoredInstallations}");

            hasLoggedInitialCacheSnapshot = true;
        }
        ReplaceIgnoredGamePaths(cacheData.IgnoredGamePaths);

        List<KnownGame> savedGames = cacheData.KnownGames
            .Where(game => !IsIgnoredGamePath(game.PathToGame))
            .Select(Normalize)
            .Where(game => !string.IsNullOrWhiteSpace(game.PathToGame))
            .ToList();
        List<KnownGame> discoveredGames = GameInstallationFinder.FindGamesCached(gameInfo)
            .Select(ToKnownGame)
            .Where(game => !IsIgnoredGamePath(game.PathToGame))
            .ToList();

        List<KnownGame> mergedGames = [];
        mergedGames.AddRange(savedGames);
        mergedGames.AddRange(discoveredGames);
        mergedGames = mergedGames
            .Where(game => !string.IsNullOrWhiteSpace(game.PathToGame))
            .Select(Normalize)
            .Where(game => !IsIgnoredGamePath(game.PathToGame))
            .DistinctBy(game => game.PathToGame, StringComparer.OrdinalIgnoreCase)
            .ToList();

        KnownGame? selectedGame = TryGetSelectedGame(gameInfo, mergedGames);
        if (selectedGame is null)
        {
            selectedGame = mergedGames.FirstOrDefault();
        }

        InstalledGames.Clear();
        foreach (KnownGame discoveredGame in mergedGames)
        {
            InstalledGames.Add(discoveredGame);
        }

        if (selectedGame is null)
        {
            SelectedGame = new KnownGame { PathToGame = string.Empty, Platform = Platform.NONE };
            NitroxUser.PreferredGamePath = string.Empty;
            NitroxUser.ClearGamePathAndPlatform();
            SaveKnownGames(gameInfo);
            return mergedGames;
        }

        PromoteInstalledGame(selectedGame);
        ApplySelection(selectedGame);
        SaveKnownGames(gameInfo);
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

        KnownGame normalizedGame = Normalize(game);
        RemoveIgnoredGamePath(normalizedGame.PathToGame);
        PromoteInstalledGame(normalizedGame);
        ApplySelection(normalizedGame);
        SaveKnownGames(gameInfo);
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

        bool deletedSelectedGame = IsGamePathSelected(normalizedGame.PathToGame);
        if (deletedSelectedGame)
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
                PromoteInstalledGame(fallbackGame);
                ApplySelection(fallbackGame);
            }
        }

        SaveKnownGames(gameInfo);
        return removed;
    }

    public void SelectGameInstallation(GameInfo gameInfo, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        SelectGameInstallation(gameInfo, new KnownGame
        {
            PathToGame = Path.GetFullPath(path),
            Platform = GetPlatformFromPath(path)
        });
    }

    private GameInstallationCacheData LoadKnownGames(GameInfo gameInfo)
    {
        try
        {
            string cacheFilePath = GetCacheFilePath(gameInfo);
            if (!File.Exists(cacheFilePath))
            {
                ignoredGamePaths.Clear();
                return new GameInstallationCacheData();
            }

            string serialized = File.ReadAllText(cacheFilePath);
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

    private void SaveKnownGames(GameInfo gameInfo)
    {
        try
        {
            Directory.CreateDirectory(NitroxUser.CachePath);
            GameInstallationCacheData cacheData = new()
            {
                KnownGames = InstalledGames.Select(Normalize).Where(game => !string.IsNullOrWhiteSpace(game.PathToGame)).ToList(),
                IgnoredGamePaths = ignoredGamePaths.ToList()
            };
            string serialized = JsonSerializer.Serialize(cacheData, jsonSerializerOptions);
            File.WriteAllText(GetCacheFilePath(gameInfo), serialized);
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

    private KnownGame? TryGetSelectedGame(GameInfo gameInfo, IEnumerable<KnownGame> discoveredGames)
    {
        string selectedPath = NitroxUser.GamePath;
        if (!string.IsNullOrWhiteSpace(selectedPath) && GameInstallationHelper.HasValidGameFolder(selectedPath, gameInfo) && !IsIgnoredGamePath(selectedPath))
        {
            return new KnownGame
            {
                PathToGame = Path.GetFullPath(selectedPath),
                Platform = NitroxUser.GamePlatform?.Platform ?? GetPlatformFromPath(selectedPath)
            };
        }

        string preferredPath = NitroxUser.PreferredGamePath;
        if (!string.IsNullOrWhiteSpace(preferredPath) && GameInstallationHelper.HasValidGameFolder(preferredPath, gameInfo) && !IsIgnoredGamePath(preferredPath))
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
        string path = NormalizePath(game.PathToGame);
        Platform platform = game.Platform == Platform.NONE ? GetPlatformFromPath(path) : game.Platform;
        return new KnownGame
        {
            PathToGame = path,
            Platform = platform
        };
    }

    private void PromoteInstalledGame(KnownGame game)
    {
        for (int i = InstalledGames.Count - 1; i >= 0; i--)
        {
            if (string.Equals(InstalledGames[i].PathToGame, game.PathToGame, StringComparison.OrdinalIgnoreCase))
            {
                InstalledGames.RemoveAt(i);
            }
        }

        InstalledGames.Insert(0, game);
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
