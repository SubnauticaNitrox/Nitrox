using System;
using System.Net.Sockets;
using LiteNetLib;

namespace NitroxClient.Communication.Exceptions
{
    public class ClientConnectionFailedException : Exception
    {
        public DisconnectReason? DisconnectReason { get; }
        public SocketError? SocketError { get; }

        public ClientConnectionFailedException(string message) : base(message)
        {
        }

        public ClientConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ClientConnectionFailedException(DisconnectInfo info)
            : base($"Connection failed: {info.Reason} (socket error: {info.SocketErrorCode})")
        {
            DisconnectReason = info.Reason;
            SocketError = info.SocketErrorCode;
        }
    }
}
