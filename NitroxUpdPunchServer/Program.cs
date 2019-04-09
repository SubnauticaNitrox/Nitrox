using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroxUpdPunchServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Communication.NetworkingLayer.LiteNetLib.LiteNetLibPunchServer server = new Communication.NetworkingLayer.LiteNetLib.LiteNetLibPunchServer(11001, "NitroxPunch");
            server.Start();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                server.Process();
                Thread.Sleep(10);
            }
        }
    }
}
