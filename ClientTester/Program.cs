using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using ClientTester.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using UnityEngine;

namespace ClientTester
{
    class Program
    {
        private static readonly string DEFAULT_IP_ADDRESS = "127.0.0.1";

        private static Vector3 clientPos = new Vector3(-50f, -2f, -38f);

        static void Main(string[] args)
        {
            String playerId1 = "sunrunner";

            //give main server a second to start up...
            System.Threading.Thread.Sleep(1000);

            MultiplayerClient mplayer1 = new MultiplayerClient(playerId1);
            mplayer1.Start(DEFAULT_IP_ADDRESS);

            CommandManager manager = new CommandManager(mplayer1);
            while (true)
            {
                manager.TakeCommand(Console.ReadLine());
            }
        }

        private static int lastX = -1;
        private static int lastY = -1;
        private static void mouseTimerTick(MultiplayerClient client)
        {
            int curX = System.Windows.Forms.Cursor.Position.X;
            int curY = System.Windows.Forms.Cursor.Position.Y;
            if (lastX != -1)
            {
                float velX = curX - lastX;
                float velY = curY - lastY;
                clientPos += new Vector3(velX / 10f, 0, velY / 10f);
                client.PacketSender.UpdatePlayerLocation(clientPos, Quaternion.identity, Optional<VehicleModel>.Empty());
            }
            lastX = curX;
            lastY = curY;
        }
    }
}
