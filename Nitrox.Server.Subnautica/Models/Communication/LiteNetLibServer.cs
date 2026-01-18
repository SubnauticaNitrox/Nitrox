using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using LiteNetLib;
using LiteNetLib.Utils;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Networking;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Communication;

internal sealed class LiteNetLibServer : IHostedService, IPacketSender, IKickPlayer
{
    private readonly NetDataWriter dataWriter = new();
    private readonly SessionManager sessionManager;
    private readonly PacketSerializationService packetSerializationService;
    private readonly PacketHandler packetHandler;
    private readonly PlayerManager playerManager;
    private readonly JoiningManager joiningManager;
    private readonly EntitySimulation entitySimulation;
    private readonly SleepManager sleepManager;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly ILogger<LiteNetLibServer> logger;
    private readonly Dictionary<SessionId, PeerContext> contextBySessionId = [];
    private readonly Lock contextBySessionIdLock = new();
    private readonly EventBasedNetListener listener;
    private readonly NetManager server;

    public LiteNetLibServer(SessionManager sessionManager, PacketSerializationService packetSerializationService, PacketHandler packetHandler, PlayerManager playerManager, JoiningManager joiningManager, EntitySimulation entitySimulation, SleepManager sleepManager, IOptions<SubnauticaServerOptions> options, ILogger<LiteNetLibServer> logger)
    {
        this.sessionManager = sessionManager;
        this.packetSerializationService = packetSerializationService;
        this.packetHandler = packetHandler;
        this.playerManager = playerManager;
        this.joiningManager = joiningManager;
        this.entitySimulation = entitySimulation;
        this.sleepManager = sleepManager;
        this.options = options;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener)
        {
            IPv6Enabled = true
        };
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (request.Data.GetString() != "nitrox")
        {
            request.Reject();
            return;
        }
        if (server.ConnectedPeersCount >= options.Value.MaxConnections)
        {
            request.Reject();
            return;
        }

        SessionManager.Session session = sessionManager.GetOrCreateSession(request.RemoteEndPoint);
        contextBySessionId.TryAdd(session.Id, new PeerContext(request.Accept()));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        listener.PeerConnectedEvent += PeerConnected;
        listener.PeerDisconnectedEvent += PeerDisconnected;
        listener.NetworkReceiveEvent += NetworkDataReceived;
        listener.ConnectionRequestEvent += OnConnectionRequest;

        server.ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length;
        server.BroadcastReceiveEnabled = true;
        server.UnconnectedMessagesEnabled = true;
        server.UpdateTime = 15;
        server.UnsyncedEvents = true;
#if DEBUG
        server.DisconnectTimeout = 300000; //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif

        server.Start(options.Value.ServerPort);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!server.IsRunning)
        {
            return;
        }

        playerManager.SendPacketToAllPlayers(new ServerStopped());
        // We want every player to receive this packet
        await Task.Delay(500, cancellationToken);
        server.Stop();
    }

    private void ClientDisconnected(INitroxConnection connection)
    {
        Player? player = playerManager.GetPlayer(connection);
        if (player == null)
        {
            joiningManager.JoiningPlayerDisconnected(connection);
            return;
        }

        sleepManager.PlayerDisconnected(player);
        playerManager.PlayerDisconnected(connection);

        Disconnect disconnect = new(player.SessionId);
        playerManager.SendPacketToAllPlayers(disconnect);

        List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            playerManager.SendPacketToAllPlayers(ownershipChange);
        }
    }

    public void ProcessIncomingData(INitroxConnection connection, Packet packet)
    {
        try
        {
            packetHandler.Process(packet, connection);
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Exception while processing packet: {packet}");
        }
    }

    private void PeerConnected(NetPeer peer) => logger.ZLogInformation($"Connection made by {peer.Address:@IP}:{peer.Port}");

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        SessionId? sessionId = null;
        lock (contextBySessionIdLock)
        {
            foreach (KeyValuePair<SessionId, PeerContext> pair in contextBySessionId)
            {
                if (pair.Value.Peer.Id == peer.Id)
                {
                    sessionId = pair.Key;
                    break;
                }
            }
        }

        if (!sessionId.HasValue)
        {
            logger.ZLogWarning($"Disconnected peer id {peer.Id} did not have an associated session id!");
            return;
        }
        sessionManager.DeleteSessionAsync(sessionId.Value);
    }

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            ProcessIncomingData(connection, packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    public async ValueTask SendPacket<T>(T packet, SessionId sessionId) where T : Packet => throw new NotImplementedException();

    public async ValueTask SendPacketToAll<T>(T packet) where T : Packet => throw new NotImplementedException();

    public async ValueTask SendPacketToOthers<T>(T packet, SessionId excludedSessionId) where T : Packet => throw new NotImplementedException();

    public async Task<bool> KickPlayer(SessionId sessionId, string reason = "")
    {
        PeerContext context;
        lock (contextBySessionIdLock)
        {
            if (!contextBySessionId.TryGetValue(sessionId, out context))
            {
                return false;
            }
        }
        await SendPacket(new PlayerKicked(reason), sessionId);
        server.DisconnectPeer(context.Peer); // This will trigger client disconnect, which will handle the session (data) migration.
        return true;
    }

    private void SendPacket(Packet packet, NetPeer peer)
    {
        using EasyPool<MemoryStream>.Lease lease = EasyPool<MemoryStream>.Rent();
        ref MemoryStream stream = ref lease.GetRef();
        stream ??= new MemoryStream(ushort.MaxValue);

        int startPos = (int)stream.Position;
        packetSerializationService.SerializeInto(packet, stream);
        int bytesWritten = (int)(stream.Position - startPos);
        Span<byte> packetData = stream.GetBuffer().AsSpan().Slice(startPos, bytesWritten);

        lock (dataWriter)
        {
            dataWriter.Reset();
            dataWriter.Put(packetData.Length);
            dataWriter.ResizeIfNeed(packetData.Length + 4);
            packetData.CopyTo(dataWriter.Data.AsSpan().Slice(4));
            dataWriter.SetPosition(packetData.Length + 4);
            peer.Send(dataWriter, (byte)packet.UdpChannel, NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
        }

        // Cleanup pooled data.
        stream.Position = 0;
    }

    internal record PeerContext(NetPeer Peer);
}
