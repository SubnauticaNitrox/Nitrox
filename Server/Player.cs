using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Tcp;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer
{
    public class Player
    {
        public String Id { get; private set; }
        public Connection Connection { get; private set; }
        public Vector3 Position { get; set; }

        public Player(String id, Connection connection)
        {
            this.Id = id;
            this.Connection = connection;
        }
    }
}
