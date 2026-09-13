using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Services;

// TODO: This manager should only handle player data. Move connection related state to other managers.
// TODO: Add version number to PlayerData.json
internal sealed partial class PlayerService(
    JsonSerializerOptions jsonOptions,
    SessionManager sessionManager,
    IBan ban,
    IServiceProvider services,
    IOptions<ServerStartOptions> startOptions,
    IOptions<SubnauticaServerOptions> options,
    ILogger<PlayerService> logger)
    : IHostedService, ISessionCleaner, ISaveState
{
    public const string PLAYER_PREFERENCES_GROUP_NAME = "PlayerPreferences";
    private readonly IBan ban = ban;
    private readonly JsonSerializerOptions jsonOptions = jsonOptions;
    private readonly TaskCompletionSource loaded = new();
    private readonly ILogger<PlayerService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ThreadSafeDictionary<SessionId, PeerId> peerIdBySessionId = [];
    private readonly ThreadSafeDictionary<PeerId, Dictionary<Type, IPlayerProperty>> playerProperties = [];
    private readonly ThreadSafeDictionary<SessionId, PlayerContext> reservations = [];

    private readonly ThreadSafeDictionary<string, SessionId> reservedPlayerNames =
    [
        // "Player" is often used to identify the local player and should not be used by any user
        new KeyValuePair<string, SessionId>("Player", (SessionId)0)
    ];

    private readonly IServiceProvider services = services;
    private readonly SessionManager sessionManager = sessionManager;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;

    /// <summary>
    ///     This value always holds the latest and highest peer id.
    ///     Is used and persisted to avoid peer id conflicts.
    /// </summary>
    private uint latestPeerId;

    public int ConnectedPlayerCount => peerIdBySessionId.Count;

    private string FilePath => Path.Combine(startOptions.Value.GetServerSavePath(), "PlayerData.json");

    /// <summary>
    ///     True if the player is reserved and authenticated by the server as a valid player.
    /// </summary>
    public PlayerContext? GetPlayerReservation(SessionId sessionId)
    {
        return reservations.TryGetValue(sessionId, out PlayerContext player) ? player : null;
    }

    /// <summary>
    ///     Returns the sessions that have been reserved as a valid player.
    /// </summary>
    public IEnumerable<SessionId> GetSessionIds()
    {
        using RentedArray<SessionId> sessions = reservations.GetKeysNoAlloc();
        foreach (SessionId session in sessions)
        {
            yield return session;
        }
    }

    public IEnumerable<SessionId> GetSessionsWhereProperty<T>(Func<T, bool> predicate) where T : IPlayerProperty
    {
        using RentedArray<SessionId> sessions = reservations.GetKeysNoAlloc();
        foreach (SessionId sessionId in sessions)
        {
            if (predicate(GetProperty<T>(sessionId)))
            {
                yield return sessionId;
            }
        }
    }

    /// <summary>
    ///     Gets sessions that are reserved as valid players, except the excluded session id.
    /// </summary>
    public IEnumerable<SessionId> GetSessionsExcept(SessionId excludedSessionId)
    {
        using RentedArray<SessionId> sessions = reservations.GetKeysNoAlloc();
        foreach (SessionId sessionId in sessions)
        {
            if (sessionId == excludedSessionId)
            {
                continue;
            }
            yield return sessionId;
        }
    }

    public async Task<MultiplayerSessionReservation> ReservePlayerContextAsync(
        SessionId sessionId,
        IPEndPoint endPoint,
        PlayerSettings playerSettings,
        AuthenticationContext authenticationContext)
    {
        if (ban.IsBanned(endPoint.Address))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.PLAYER_BANNED;
            return new MultiplayerSessionReservation(sessionId, rejectedState, ban.GetBanRejectionDetail(endPoint.Address));
        }

        if (sessionManager.GetSessionCount() > options.Value.MaxConnections)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            return new MultiplayerSessionReservation(sessionId, rejectedState);
        }

        if (!string.IsNullOrEmpty(options.Value.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != options.Value.ServerPassword))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
            return new MultiplayerSessionReservation(sessionId, rejectedState);
        }

        if (!PlayerNameRegex().IsMatch(authenticationContext.Username))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
            return new MultiplayerSessionReservation(sessionId, rejectedState);
        }

        string playerName = authenticationContext.Username;
        if (!reservedPlayerNames.TryAdd(playerName, sessionId))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
            return new MultiplayerSessionReservation(sessionId, rejectedState);
        }

        // TODO: Better authentication than player name ...
        PeerId? assignedPlayerIdForSession = TryGetPeerId(playerName);

        PlayerContext playerContext = await CreateOrResetPlayerDataAsync(sessionId, assignedPlayerIdForSession, playerName, playerSettings);

        if (GetProperty<IsPermaDeathProperty>(sessionId).Value && options.Value.IsHardcore())
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.HARDCORE_PLAYER_DEAD;
            return new MultiplayerSessionReservation(sessionId, rejectedState);
        }

        reservations.Add(sessionId, playerContext);

        return new MultiplayerSessionReservation(sessionId);
    }

    public bool TryGetPlayerByName(string playerName, [NotNullWhen(true)] out SessionId? foundPlayer)
    {
        if (reservedPlayerNames.TryGetValue(playerName, out SessionId result))
        {
            foundPlayer = result;
            return true;
        }
        foundPlayer = null;
        return false;
    }

    /// <summary>
    ///     Gets a player property for the peer id, if the player exists.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <see cref="peerId" /> is less than 1.
    /// </exception>
    public T? GetProperty<T>(PeerId peerId) where T : IPlayerProperty
    {
        loaded.Task.GetAwaiter().GetResult();
        if ((uint)peerId < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(peerId), "Value must be more than zero");
        }
        if (!playerProperties.TryGetValue(peerId, out Dictionary<Type, IPlayerProperty>? properties))
        {
            throw new InvalidOperationException($"Player properties are not yet initialized for {nameof(PeerId)} #{peerId}");
        }
        return (T)properties[typeof(T)];
    }

    /// <summary>
    ///     Gets a player property for the session id, if the player exists.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <see cref="sessionId" /> is less than 1.
    /// </exception>
    public T? GetProperty<T>(SessionId sessionId) where T : IPlayerProperty
    {
        if ((uint)sessionId < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sessionId), "Value must be more than zero");
        }
        if (!peerIdBySessionId.TryGetValue(sessionId, out PeerId peerId))
        {
            throw new InvalidOperationException($"Player properties are not yet initialized for {nameof(SessionId)} #{sessionId}");
        }
        return GetProperty<T>(peerId);
    }

    public PeerId GetPeerId(SessionId sessionId) => peerIdBySessionId[sessionId];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using FileStream stream = File.OpenRead(FilePath);
            JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            JsonElement playersElement = document.RootElement.GetProperty("Players");
            foreach (JsonElement playerElement in playersElement.EnumerateArray())
            {
                IPlayerProperty[] properties;
                await using (AsyncServiceScope scope = services.CreateAsyncScope())
                {
                    properties = scope.ServiceProvider.GetRequiredService<IEnumerable<IPlayerProperty>>().ToArray();
                }
                foreach (IPlayerProperty property in properties)
                {
                    if (property is IConnectedPlayerProperty)
                    {
                        continue;
                    }
                    JsonElement selectedElement = playerElement;
                    if (property.GetGroupName() is { Length: > 0 } groupName && !playerElement.TryGetProperty(groupName, out selectedElement))
                    {
                        continue;
                    }
                    string propertyName = property.GetName();
                    // This allows some properties to not be available. We'll use default values.
                    if (!selectedElement.TryGetProperty(propertyName, out JsonElement jsonProperty))
                    {
                        continue;
                    }
                    property.Value = jsonProperty.Deserialize(property.GetTypeOfValue(), jsonOptions);
                }
                playerProperties[(PeerId)playerElement.GetProperty("Id").GetUInt32()] = properties.ToDictionary(p => p.GetType(), p => p);
            }
            if (playerProperties.Count > 0)
            {
                Interlocked.Exchange(ref latestPeerId, (uint)playerProperties.Keys.Max());
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            // No player file yet.
        }
        catch (Exception ex)
        {
            playerProperties.Clear();
            Interlocked.Exchange(ref latestPeerId, 0);
            logger.ZLogError(ex, $"Could not load player list, starting with an empty one");
        }
        finally
        {
            loaded.TrySetResult();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    ///     Gets all the properties of the given type from all connected players.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<(SessionId, T)> GetProperties<T>() where T : IPlayerProperty
    {
        foreach (SessionId sessionId in GetSessionIds())
        {
            yield return (sessionId, GetProperty<T>(sessionId));
        }
    }

    [GeneratedRegex(NitroxConstants.PLAYER_NAME_VALID_REGEX, RegexOptions.NonBacktracking)]
    private static partial Regex PlayerNameRegex();

    private async Task<PlayerContext> CreateOrResetPlayerDataAsync(SessionId sessionId, PeerId? existingPeerId, string playerName, PlayerSettings playerSettings)
    {
        await loaded.Task;
        reservations.Remove(sessionId);

        PeerId assignedPeerId;
        Dictionary<Type, IPlayerProperty> properties;

        if (existingPeerId.HasValue)
        {
            assignedPeerId = existingPeerId.Value;
            properties = playerProperties[assignedPeerId];
        }
        else
        {
            // Create new persisted record for the player.
            uint newPeerIdNum = Interlocked.Increment(ref latestPeerId);
            assignedPeerId = (PeerId)newPeerIdNum;
            // Initialize new properties for the player record.
            await using AsyncServiceScope scope = services.CreateAsyncScope();
            properties = scope.ServiceProvider.GetRequiredService<IEnumerable<IPlayerProperty>>().ToDictionary(p => p.GetType(), p => p);
            playerProperties[assignedPeerId] = properties;
        }
        logger.ZLogTrace($"Assigned peer id #{assignedPeerId} to session #{sessionId} - IsExistingPeer: {existingPeerId.HasValue}");

        peerIdBySessionId[sessionId] = assignedPeerId;

        // Always reset "connected player properties" as by design.
        foreach (IPlayerProperty prop in properties.Values)
        {
            if (prop is IConnectedPlayerProperty implProp)
            {
                await implProp.ResetAsync();
            }
        }

        // These properties are set on connect, so we do that here. Other properties are set later during gameplay, or have a proper default value.
        GetProperty<IsNewPlayerProperty>(sessionId).Value = !existingPeerId.HasValue;
        GetProperty<VisitCountProperty>(sessionId).Increment();
        GetProperty<SessionIdProperty>(sessionId).Value = sessionId;
        GetProperty<NameProperty>(sessionId).Value = playerName;
        IntroCinematicMode introCinematicMode = existingPeerId.HasValue ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;
        GetProperty<IntroCinematicModeProperty>(sessionId).Value = introCinematicMode;
        return GetProperty<ContextProperty>(sessionId).Value = new(playerName,
                                                                   sessionId,
                                                                   GetProperty<GameObjectIdProperty>(sessionId).Value,
                                                                   !existingPeerId.HasValue,
                                                                   playerSettings,
                                                                   false,
                                                                   GetProperty<GameModeProperty>(sessionId).Value,
                                                                   null,
                                                                   introCinematicMode,
                                                                   GetProperty<AnimationProperty>(sessionId).Value);
    }

    private PeerId? TryGetPeerId(string playerName)
    {
        foreach (PeerId peerId in playerProperties.Keys)
        {
            if (GetProperty<NameProperty>(peerId).Value == playerName)
            {
                return peerId;
            }
        }
        return null;
    }

    Task IEvent<ISessionCleaner.Args>.OnEventAsync(ISessionCleaner.Args args)
    {
        reservations.Remove(args.Session.Id);
        peerIdBySessionId.Remove(args.Session.Id);

        string? playerName = null;
        foreach (KeyValuePair<string, SessionId> pair in reservedPlayerNames)
        {
            if (pair.Value == args.Session.Id)
            {
                playerName = pair.Key;
                break;
            }
        }
        if (playerName != null)
        {
            reservedPlayerNames.Remove(playerName);
            logger.ZLogInformation($"Player '{playerName}' #{args.Session.Id} left the game");
        }
        else
        {
            logger.ZLogInformation($"Session #{args.Session.Id} left the game");
        }

        return Task.CompletedTask;
    }

    async Task IEvent<ISaveState.Args>.OnEventAsync(ISaveState.Args args)
    {
        JsonObject document = new();
        JsonArray players = new();
        foreach (KeyValuePair<PeerId, Dictionary<Type, IPlayerProperty>> keyValuePair in playerProperties)
        {
            JsonObject player = new();
            player.Add("Id", JsonSerializer.SerializeToNode(keyValuePair.Key, jsonOptions));
            foreach (IGrouping<string, IPlayerProperty> grouping in keyValuePair.Value.Values
                                                                                .Select(p => new
                                                                                {
                                                                                    Prop = p,
                                                                                    Name = p.GetName()
                                                                                })
                                                                                .OrderByDescending(p => p.Name.Contains("Id", StringComparison.Ordinal) || p.Name.Contains("Name", StringComparison.Ordinal))
                                                                                .ThenBy(p => p.Name)
                                                                                .Select(p => p.Prop)
                                                                                .GroupBy(p => p.GetGroupName()))
            {
                JsonObject? group = grouping.Key != "" ? new JsonObject() : null;
                foreach (IPlayerProperty property in grouping)
                {
                    if (property is IConnectedPlayerProperty)
                    {
                        continue;
                    }

                    JsonObject selectedObj = group ?? player;
                    selectedObj.Add(property.GetName(), JsonSerializer.SerializeToNode(property.Value, jsonOptions));
                }
                if (group != null)
                {
                    player.Add(grouping.Key, group);
                }
            }
            players.Add(player);
        }
        document.Add("Players", players);
        await using FileStream jsonStream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(jsonStream, document, jsonOptions);
    }
}
