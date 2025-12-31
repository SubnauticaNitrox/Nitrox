using System.Buffers;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets;

namespace Nitrox.Server.Subnautica.Models.Communication;

internal sealed class LiteNetLibServer : IHostedService
{
    private readonly PacketHandler packetHandler;
    private readonly PlayerManager playerManager;
    private readonly JoiningManager joiningManager;
    private readonly EntitySimulation entitySimulation;
    private readonly SleepManager sleepManager;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly ILogger<LiteNetLibServer> logger;
    private readonly Dictionary<int, INitroxConnection> connectionsByRemoteIdentifier = [];
    private readonly EventBasedNetListener listener;
    private readonly NetManager server;

    static LiteNetLibServer()
    {
        Packet.InitSerializer();
    }

    public LiteNetLibServer(PacketHandler packetHandler, PlayerManager playerManager, JoiningManager joiningManager, EntitySimulation entitySimulation, SleepManager sleepManager, IOptions<SubnauticaServerOptions> options, ILogger<LiteNetLibServer> logger)
    {
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

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (server.ConnectedPeersCount < options.Value.MaxConnections)
        {
            request.AcceptIfKey("nitrox");
        }
        else
        {
            request.Reject();
        }
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

    protected void ClientDisconnected(INitroxConnection connection)
    {
        Player? player = playerManager.GetPlayer(connection);
        if (player == null)
        {
            joiningManager.JoiningPlayerDisconnected(connection);
            return;
        }

        sleepManager.PlayerDisconnected(player);
        playerManager.PlayerDisconnected(connection);

        Disconnect disconnect = new(player.Id);
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

    private void PeerConnected(NetPeer peer)
    {
        LiteNetLibConnection connection = new(peer, logger);
        lock (connectionsByRemoteIdentifier)
        {
            connectionsByRemoteIdentifier[peer.Id] = connection;
        }
    }

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        INitroxConnection connection = GetConnection(peer.Id);
        if (connection == null)
        {
            return;
        }
        ClientDisconnected(connection);
    }

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            INitroxConnection connection = GetConnection(peer.Id);
            ProcessIncomingData(connection, packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private INitroxConnection? GetConnection(int remoteIdentifier)
    {
        INitroxConnection connection;
        lock (connectionsByRemoteIdentifier)
        {
            connectionsByRemoteIdentifier.TryGetValue(remoteIdentifier, out connection);
        }

        return connection;
    }
}
