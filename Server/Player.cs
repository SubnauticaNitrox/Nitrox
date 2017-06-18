using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer
{
    public class Player
    {
        public static int PLAYER_DISTANCE_FROM_ACTION = 500;
        public static long DEFERRED_PLAYER_ACTION_PAUSE_MILLISECONDS = 10;


        public String Id { get; private set; }
        public Connection Connection { get; private set; }
        public Vector3 Position { get; set; }
        public TimedQueue<PlayerActionPacket> DeferredPackets { get; private set; }

        public Player(String id, Connection connection)
        {
            this.Id = id;
            this.Connection = connection;

            long deferPeriod = DEFERRED_PLAYER_ACTION_PAUSE_MILLISECONDS * 1000 * 1000;
            this.DeferredPackets = new TimedQueue<PlayerActionPacket>(deferPeriod);
        }

        public bool inRangeOfAction(Vector3 actionPositon)
        {
            if(Position == null)
            {
                return false;
            }

            double distance = Math.Sqrt(Math.Pow((actionPositon.X - Position.X), 2) + Math.Pow((actionPositon.Z - Position.Z), 2));

            return distance <= PLAYER_DISTANCE_FROM_ACTION;
        }

    }
}
