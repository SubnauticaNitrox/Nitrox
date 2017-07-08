using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
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

            while (true)
            {
                List<String> cmd = Regex.Matches(Console.ReadLine(), @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();
                switch (cmd[0])
                {
                    case "chat":
                        if (cmd.Count < 1) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        if (cmd.Count >= 3)
                        {
                            mplayer1.PacketSender.SendChatMessage(String.Join(" ", cmd.Skip(1).ToArray())); //does not support double spaces!
                        }
                        else
                        {
                            mplayer1.PacketSender.SendChatMessage(cmd[1]);
                        }
                        break;
                    case "pickup":
                        if (cmd.Count < 5) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        mplayer1.PacketSender.PickupItem(GetVectorFromArgs(cmd, 2), cmd[1], "");
                        break;
                    case "build":
                        if (cmd.Count < 5) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        if (cmd.Count > 5)
                        {
                            mplayer1.PacketSender.BuildItem(cmd[1], GetVectorFromArgs(cmd, 2), GetQuaternionFromArgs(cmd, 5));
                        }
                        else
                        {
                            mplayer1.PacketSender.BuildItem(cmd[1], GetVectorFromArgs(cmd, 2), Quaternion.identity);
                        }
                        break;
                    case "construct":
                        if (cmd.Count < 5) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        mplayer1.PacketSender.ChangeConstructionAmount(GetVectorFromArgs(cmd, 2), float.Parse(cmd[1]), 0, 0);
                        break;
                    case "drop":
                        if (cmd.Count < 5) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        mplayer1.PacketSender.DropItem(cmd[1], GetVectorFromArgs(cmd, 2), Vector3.zero);
                        break;
                    case "move":
                        Console.WriteLine("Mouse is now attached. Press any key to exit");
                        Timer mouseTimer = new Timer();
                        mouseTimer.Elapsed += delegate { mouseTimerTick(mplayer1); };
                        mouseTimer.Interval = 50;
                        mouseTimer.Start();
                        Console.ReadKey();
                        lastX = -1;
                        lastY = -1;
                        mouseTimer.Stop();
                        break;
                    case "movez":
                        if (cmd.Count < 2) { Console.WriteLine($"\"{cmd[0]}\" does not take {cmd.Count - 1} arguments"); break; }
                        clientPos.z = float.Parse(cmd[1]);
                        mplayer1.PacketSender.UpdatePlayerLocation(clientPos, Quaternion.identity, Optional<VehicleModel>.Empty());
                        break;
                    case "pos":
                        Console.WriteLine(clientPos.ToString());
                        break;
                    case "help":
                        Console.WriteLine("chat <message>");
                        Console.WriteLine("pickup <gameobjectname> <x> <y> <z>");
                        Console.WriteLine("build <techtype> <x> <y> <z> [xrot] [yrot] [zrot]");
                        Console.WriteLine("construct <amount> <x> <y> <z>");
                        Console.WriteLine("drop <techtype> <x> <y> <z>");
                        Console.WriteLine("move");
                        Console.WriteLine("movez <z>");
                        Console.WriteLine("pos");
                        break;
                }
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

        private static Vector3 GetVectorFromArgs(List<String> args, int pos)
        {
            return new Vector3(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }

        private static Quaternion GetQuaternionFromArgs(List<String> args, int pos)
        {
            return Quaternion.Euler(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }
    }
}
