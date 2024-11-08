using System;
using System.Net;
using LiteNetLib.Layers;
using LiteNetLib;
using System.Collections.Generic;

namespace NitroxServer.Communication.LiteNetLib;

public class PortCheckerSupport : PacketLayerBase
{
    public PortCheckerSupport() : base(0)
    {

    }
    
    public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
    {
        Log.Info("$\"{endPoint} - {length} bytes: {BitConverter.ToString(data).Replace('-', '\\0')}\"");
    }
    public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
    {
        
    }
}
