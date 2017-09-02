using NitroxServer.Communication;

namespace NitroxServer.GameLogic
{
    public class Logic
    {
        public AI AI { get; private set; }
        
        public Logic(TcpServer tcpServer)
        {
            this.AI = new AI(tcpServer);
        }
    }
}
