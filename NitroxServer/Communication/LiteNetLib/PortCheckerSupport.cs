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
    public static bool active;
    public NetManager netManager;
    public PortCheckerSupport() : base(0)
    {
        active = false;
    }
    
    public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
    {
        if (active)
        {
            Log.Warn("WARNING: Port Checker must be disabled to allow players to join the server");
            byte[] datacopy = data;
            data = [];
            netManager.SendUnconnectedMessage(data, endPoint);
        }
        
    }
    public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
    {

    }
}
