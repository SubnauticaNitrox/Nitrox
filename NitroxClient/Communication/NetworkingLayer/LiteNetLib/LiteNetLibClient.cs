using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.Abstract;
using NitroxClient.Debuggers;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib;

public class LiteNetLibClient : IClient
{
    public bool IsConnected { get; private set; }

    private readonly AutoResetEvent connectedEvent = new(false);
    private readonly NetDataWriter dataWriter = new();
    private readonly PacketReceiver packetReceiver;
    private readonly INetworkDebugger networkDebugger;

    private NetManager client;

    public LiteNetLibClient(PacketReceiver packetReceiver, INetworkDebugger networkDebugger = null)
    {
        this.packetReceiver = packetReceiver;
        this.networkDebugger = networkDebugger;
    }

    public async Task StartAsync(string ipAddress, int serverPort)
    {
        Log.Info("Initializing LiteNetLibClient...");

        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        EventBasedNetListener listener = new EventBasedNetListener();
        listener.PeerConnectedEvent += Connected;
        listener.PeerDisconnectedEvent += Disconnected;
        listener.NetworkReceiveEvent += ReceivedNetworkData;

        client = new NetManager(listener)
        {
            UpdateTime = 15,
#if DEBUG
            DisconnectTimeout = 300000 //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif
        };

        await Task.Run(() =>
        {
            client.Start();
            client.Connect(ipAddress, serverPort, "nitrox");
        });

        connectedEvent.WaitOne(2000);
        connectedEvent.Reset();
    }

    public void Send(Packet packet)
    {
        byte[] packetData = packet.Serialize();
        dataWriter.Reset();
        dataWriter.Put(packetData.Length);
        dataWriter.Put(packetData);

        networkDebugger?.PacketSent(packet, dataWriter.Length);
        client.SendToAll(dataWriter, NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
    }

    public void Stop()
    {
        IsConnected = false;
        client.Stop();
    }

    /// <summary>
    /// This should be called <b>once</b> each game tick
    /// </summary>
    public void PollEvents() => client.PollEvents();

    private void ReceivedNetworkData(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            packetReceiver.Add(packet);
            networkDebugger?.PacketReceived(packet, packetDataLength);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private void Connected(NetPeer peer)
    {
        // IsConnected must happen before Set() so that its state is noticed WHEN we unblock the thread (cf. connectedEvent.WaitOne(...))
        IsConnected = true;
        connectedEvent.Set();
        Log.Info("Connected to server");
    }

    private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Check must happen before IsConnected is set to false, so that it doesn't send an exception when we aren't even ingame
        if (Multiplayer.Active)
        {
            Modal.Get<LostConnectionModal>()?.Show();
        }

        IsConnected = false;
        Log.Info("Disconnected from server");
    }
}
