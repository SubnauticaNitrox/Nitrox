using System;
using System.Net;
using LiteNetLib.Layers;
using LiteNetLib;
using System.Collections.Generic;
using LiteNetLib.Utils;
using System.Net.Sockets;
namespace NitroxServer.Communication.LiteNetLib;

public class PortCheckerSupport : PacketLayerBase
{
    public bool active;
    public NetManager netManager;
    public PortCheckerSupport() : base(0)
    {
        active = true;
    }
    
    public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
    {
        if (active)
        {
            Log.Info("Incoming packet");
            UdpClient client = new();
            client.Send(data, length, endPoint);
            client.Dispose();
        }
        
    }
    public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
    {

    }
    public void Deactivate()
    {
        active = false;
        Log.Info("Deactivated port forward checker");
    }
    public void Activate()
    {
        active = true;
        Log.Info("Activated port forward checker");
    }
}
