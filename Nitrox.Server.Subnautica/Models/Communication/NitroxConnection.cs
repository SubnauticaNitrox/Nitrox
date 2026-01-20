using System.Net;
using Nitrox.Model.Core;
using Nitrox.Model.Packets.Processors.Abstract;

namespace Nitrox.Server.Subnautica.Models.Communication;

public interface INitroxConnection : IProcessorContext
{
    SessionId SessionId { get; }
    IPEndPoint Endpoint { get; }

    NitroxConnectionState State { get; }
}
