using System.Net.Sockets;
using NitroxModel.Tcp;

namespace NitroxServer.Communication
{
    public class PlayerConnection : Connection
    {
        public Player Player { get; set; }

        public PlayerConnection(Socket socket) : base(socket)
        {
        }
    }
}
