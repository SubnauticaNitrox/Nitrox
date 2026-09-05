using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;
using Nitrox.Model.Core;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets.Core;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class LiteNetLibServerService : IHostedService, IPacketSender, IKickPlayer, ISessionCleaner
{
    private readonly Dictionary<int, PeerContext> contextByPeerId = [];
    private readonly Dictionary<SessionId, PeerContext> contextBySessionId = [];
    private readonly Lock contextLock = new();
    private readonly NetDataWriter dataWriter = new();
    private readonly EventBasedNetListener listener;
    private readonly ILogger<LiteNetLibServerService> logger;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PacketRegistryService packetRegistryService;
    private readonly PacketSerializationService packetSerializationService;
    private readonly PlayerManager playerManager;
    private readonly NetManager server;
    private readonly SessionManager sessionManager;
    private readonly TaskQueueService taskQueueService;

    public LiteNetLibServerService(PlayerManager playerManager, SessionManager sessionManager, PacketSerializationService packetSerializationService, PacketRegistryService packetRegistryService, IOptions<SubnauticaServerOptions> options,
                                   TaskQueueService taskQueueService, ILogger<LiteNetLibServerService> logger)
    {
        this.playerManager = playerManager;
        this.sessionManager = sessionManager;
        this.packetSerializationService = packetSerializationService;
        this.packetRegistryService = packetRegistryService;
        this.options = options;
        this.taskQueueService = taskQueueService;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener, NitroxEnvironment.IsReleaseMode ? new Crc32cLayer() : null)
        {
            UseNativeSockets = true,
            IPv6Enabled = true
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        listener.PeerDisconnectedEvent += PeerDisconnected;
        listener.NetworkReceiveEvent += NetworkDataReceived;
        listener.ConnectionRequestEvent += OnConnectionRequest;

        server.ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length;
        server.BroadcastReceiveEnabled = true;
        server.UnconnectedMessagesEnabled = true;
        server.UpdateTime = 15;
        server.UnsyncedEvents = true;
#if DEBUG
        server.DisconnectTimeout = 300_000; //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#else
        server.DisconnectTimeout = 30_000; // 30 seconds; prevents false disconnects when post-sync game-loading stalls LiteNetLib briefly
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
        try
        {
            await Task.Delay(100, CancellationToken.None); // Gives some time for the last few packets to send out.
        }
        finally
        {
            server.Stop();
        }
    }

    public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet
    {
        PeerContext? context;
        lock (contextLock)
        {
            contextBySessionId.TryGetValue(sessionId, out context);
        }
        if (context == null)
        {
            logger.ZLogWarning($"Unable to send packet {typeof(T)} because no context is set for session #{sessionId}");
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
        await Task.Delay(100); // Give time for LiteNetLib to send the packet out before disconnecting. Otherwise, no kick modal will show on client.
        server.DisconnectPeer(context.Peer); // This will trigger client disconnect, which will handle the session (data) migration.
        return true;
    }

    async Task IEvent<ISessionCleaner.Args>.OnEventAsync(ISessionCleaner.Args args)
    {
        Disconnect disconnect = new(args.Session.Id);
        await SendPacketToAllAsync(disconnect);
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
        NetPeer peer = request.Accept();
        PeerContext context = new(session.Id, peer);
        lock (contextLock)
        {
            contextBySessionId.TryAdd(session.Id, context);
            contextByPeerId.TryAdd(peer.Id, context);
        }
    }

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

        if (!taskQueueService.TryQueue(sessionManager.RemoveSessionAsync(context.SessionId)))
        {
            logger.ZLogWarning($"Failed to queue client disconnect task for {peer as EndPoint:@EndPoint}");
        }
    }

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet? packet = Packet.Deserialize(packetData);
            if (packet == null)
            {
                return;
            }
            PeerContext context;
            lock (contextLock)
            {
                contextByPeerId.TryGetValue(peer.Id, out context);
            }
            if (context == null)
            {
                return;
            }

            if (!taskQueueService.TryQueue(ProcessPacket(context, packet)))
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
        PacketProcessorsInvoker.Entry processor = packetRegistryService.GetProcessor(packetType);

        try
        {
            switch (GetProcessorTarget(processor, peerContext.SessionId, playerManager, out Models.Player? player))
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
                            context = new AuthProcessorContext(player!, this); // ! operator: Player can't be null if AUTHENTICATED.
                        }
                        else
                        {
                            context.Sender = player!; // ! operator: Player can't be null if AUTHENTICATED.
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

        static ProcessorTarget GetProcessorTarget(PacketProcessorsInvoker.Entry? processor, SessionId sessionId, PlayerManager playerManager, out Models.Player? player)
        {
            player = null;
            if (processor == null)
            {
                return ProcessorTarget.INVALID;
            }
            if (typeof(IAuthPacketProcessor).IsAssignableFrom(processor.InterfaceType) && sessionId is { IsPlayer: true } && playerManager.TryGetPlayerBySessionId(sessionId, out player))
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

    private record PeerContext(SessionId SessionId, NetPeer Peer);
}
