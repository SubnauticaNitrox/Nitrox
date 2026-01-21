using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Channels;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Nitrox.Model.Core;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets.Core;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Communication;

internal sealed class LiteNetLibServer : IHostedService, IPacketSender, IKickPlayer
{
    private readonly Dictionary<int, PeerContext> contextByPeerId = [];
    private readonly Dictionary<SessionId, PeerContext> contextBySessionId = [];
    private readonly Lock contextLock = new();
    private readonly NetDataWriter dataWriter = new();
    private readonly EventBasedNetListener listener;
    private readonly ILogger<LiteNetLibServer> logger;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PacketRegistryService packetRegistryService;
    private readonly PacketSerializationService packetSerializationService;
    private readonly PlayerManager playerManager;
    private readonly NetManager server;
    private readonly SessionManager sessionManager;
    private readonly Channel<Task> taskChannel = Channel.CreateUnbounded<Task>();

    public LiteNetLibServer(PlayerManager playerManager, SessionManager sessionManager, PacketSerializationService packetSerializationService, PacketRegistryService packetRegistryService, IOptions<SubnauticaServerOptions> options,
                            ILogger<LiteNetLibServer> logger)
    {
        this.playerManager = playerManager;
        this.sessionManager = sessionManager;
        this.packetSerializationService = packetSerializationService;
        this.packetRegistryService = packetRegistryService;
        this.options = options;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener) { IPv6Enabled = true };
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

        await SendPacketToAllAsync(new ServerStopped());
        // We want every player to receive this packet
        await Task.Delay(500, cancellationToken);
        taskChannel.Writer.TryComplete();
        await foreach (Task task in taskChannel.Reader.ReadAllAsync(cancellationToken))
        {
            await task;
        }
        server.Stop();
    }

    public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet
    {
        PeerContext? context;
        lock (contextLock)
        {
            if (!contextBySessionId.TryGetValue(sessionId, out context))
            {
                return ValueTask.CompletedTask;
            }
        }
        if (context == null)
        {
            return ValueTask.CompletedTask;
        }
        SendPacket(packet, context.Peer);
        return ValueTask.CompletedTask;
    }

    public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet
    {
        PeerContext[] contexts = [];
        int i = 0;
        try
        {
            lock (contextLock)
            {
                int count = contextBySessionId.Count;
                contexts = ArrayPool<PeerContext>.Shared.Rent(count);
                foreach (PeerContext? peerContext in contextBySessionId.Values)
                {
                    contexts[i++] = peerContext;
                }
            }
            for (int j = 0; j < i; j++)
            {
                SendPacket(packet, contexts[j].Peer);
            }
        }
        finally
        {
            ArrayPool<PeerContext>.Shared.Return(contexts);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet
    {
        PeerContext[] contexts = [];
        int i = 0;
        try
        {
            lock (contextLock)
            {
                int count = contextBySessionId.Count;
                contexts = ArrayPool<PeerContext>.Shared.Rent(count);
                foreach (PeerContext? peerContext in contextBySessionId.Values)
                {
                    contexts[i++] = peerContext;
                }
            }
            for (int j = 0; j < i; j++)
            {
                if (contexts[j].SessionId == excludedSessionId)
                {
                    continue;
                }
                SendPacket(packet, contexts[j].Peer);
            }
        }
        finally
        {
            ArrayPool<PeerContext>.Shared.Return(contexts);
        }
        return ValueTask.CompletedTask;
    }

    public async Task<bool> KickPlayer(SessionId sessionId, string reason = "")
    {
        PeerContext context;
        lock (contextLock)
        {
            if (!contextBySessionId.TryGetValue(sessionId, out context))
            {
                return false;
            }
        }
        await SendPacketAsync(new PlayerKicked(reason), sessionId);
        server.DisconnectPeer(context.Peer); // This will trigger client disconnect, which will handle the session (data) migration.
        return true;
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
        lock (contextLock)
        {
            NetPeer peer = request.Accept();
            PeerContext context = new(session.Id, peer);
            contextBySessionId.TryAdd(session.Id, context);
            contextByPeerId.TryAdd(peer.Id, context);
        }
    }

    private void PeerConnected(NetPeer peer) => logger.ZLogInformation($"Connection made by {peer.Address:@IP}:{peer.Port}");

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        PeerContext? context;
        lock (contextLock)
        {
            if (contextByPeerId.Remove(peer.Id, out context) && context != null)
            {
                contextBySessionId.Remove(context.SessionId);
            }
        }
        if (context == null)
        {
            logger.ZLogWarning($"Disconnected peer id {peer.Id} did not have an associated session id!");
            return;
        }

        if (!taskChannel.Writer.TryWrite(sessionManager.DeleteSessionAsync(context.SessionId)))
        {
            logger.ZLogWarning($"Failed to queue client disconnect task for {peer as EndPoint:@EndPoint}");
        }

        // TODO: Handle disconnect via app event:
        // Player? player = playerManager.GetPlayer(connection);
        // if (player == null)
        // {
        //     joiningManager.JoiningPlayerDisconnected(connection);
        //     return;
        // }
        // sleepManager.PlayerDisconnected(player);
        // playerManager.PlayerDisconnected(connection);
        // Disconnect disconnect = new(player.SessionId);
        // playerManager.SendPacketToAllPlayers(disconnect);
        // List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);
        // if (ownershipChanges.Count > 0)
        // {
        //     SimulationOwnershipChange ownershipChange = new(ownershipChanges);
        //     playerManager.SendPacketToAllPlayers(ownershipChange);
        // }
    }

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            PeerContext context;
            lock (contextLock)
            {
                contextByPeerId.TryGetValue(peer.Id, out context);
            }
            if (context == null)
            {
                return;
            }

            if (!taskChannel.Writer.TryWrite(ProcessPacket(context, packet)))
            {
                logger.ZLogError($"Failed to queue packet processor task for packet type {packet.GetType().Name:@TypeName} from {peer.Address:@Address}:{peer.Port:@Port}");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private async Task ProcessPacket(PeerContext peerContext, Packet packet)
    {
        Type packetType = packet.GetType();
        logger.ZLogTrace($"Incoming packet {packetType.Name:@TypeName} by session #{peerContext.SessionId:@SessionId}");
        PacketProcessorsInvoker.Entry? processor = packetRegistryService.GetProcessor(packetType);
        if (processor == null)
        {
            return;
        }

        try
        {
            switch (GetProcessorTarget(processor, peerContext.SessionId, playerManager, out Player? player))
            {
                case ProcessorTarget.ANONYMOUS:
                    using (EasyPool<AnonProcessorContext>.Lease lease = EasyPool<AnonProcessorContext>.Rent())
                    {
                        ref AnonProcessorContext context = ref lease.GetRef();
                        if (context == null)
                        {
                            context = new AnonProcessorContext((peerContext.SessionId, peerContext.Peer), this);
                        }
                        else
                        {
                            context.Sender = (peerContext.SessionId, peerContext.Peer);
                        }
                        await processor.Execute(context, packet);
                    }
                    break;
                case ProcessorTarget.AUTHENTICATED:
                    using (EasyPool<AuthProcessorContext>.Lease lease = EasyPool<AuthProcessorContext>.Rent())
                    {
                        ref AuthProcessorContext context = ref lease.GetRef();
                        if (context == null)
                        {
                            context = new AuthProcessorContext(player, this);
                        }
                        else
                        {
                            context.Sender = player;
                        }
                        await processor.Execute(context, packet);
                    }
                    break;
                default:
                    logger.ZLogWarning($"Received invalid, unauthenticated packet: {packetType.Name:@TypeName}");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error in packet processor {processor.GetType().Name:@TypeName}");
        }

        static ProcessorTarget GetProcessorTarget(PacketProcessorsInvoker.Entry? processor, SessionId session, PlayerManager playerManager, [NotNullIfNotNull(nameof(player))] out Player? player)
        {
            player = null;
            if (processor == null)
            {
                return ProcessorTarget.INVALID;
            }
            if (typeof(IAuthPacketProcessor).IsAssignableFrom(processor.InterfaceType) && session is { IsPlayer: true } && playerManager.TryGetPlayerBySessionId(session, out player))
            {
                return ProcessorTarget.AUTHENTICATED;
            }
            if (typeof(IAnonPacketProcessor).IsAssignableFrom(processor.InterfaceType))
            {
                return ProcessorTarget.ANONYMOUS;
            }
            return ProcessorTarget.INVALID;
        }
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

    private enum ProcessorTarget
    {
        INVALID,
        ANONYMOUS,
        AUTHENTICATED
    }

    private record PeerContext(SessionId SessionId, NetPeer Peer)
    {
        public INitroxConnection Connection { get; init; } = new LiteNetLibConnection(SessionId, Peer, NullLogger.Instance);
    }
}
