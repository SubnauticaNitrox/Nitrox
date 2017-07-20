using NitroxModel.Packets;
using NitroxServer.GameLogic;
using System;

namespace NitroxServer
{
    public class Server
    {
        private Listener listener;
        private TimeKeeper timeKeeper;

        public Server()
        {
            this.listener = new Listener();
            this.timeKeeper = new TimeKeeper();

            listener.PlayerAuthenticated += Listener_PlayerAuthenticated;
        }

        public void Start()
        {
            listener.Start();
        }
        
        private void Listener_PlayerAuthenticated(object sender, System.EventArgs e)
        {
            Console.WriteLine("sending time: " + timeKeeper.GetCurrentTime());
            listener.SendPacketToAllPlayers(new TimeChange(timeKeeper.GetCurrentTime()));
        }
    }
}
