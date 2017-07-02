using System;
using UnityEngine;

namespace ClientTester
{
    class Program
    {
        private static readonly string DEFAULT_IP_ADDRESS = "127.0.0.1";

        static void Main(string[] args)
        {
            String playerId1 = "sunrunner";

            //give main server a second to start up...
            System.Threading.Thread.Sleep(1000);

            MultiplayerClient mplayer1 = new MultiplayerClient(playerId1);
            mplayer1.Start(DEFAULT_IP_ADDRESS);

            while (true)
            {
                Console.ReadLine();
               // mplayer1.SendChatMessage("Get gud noob");
                 mplayer1.PacketSender.PickupItem(new Vector3(-50.6f, -4.8f, -38.0f), "Coral_reef_purple_mushrooms_01_04(Clone)", "AcidMushroom");
                //mplayer1.dropItem("BaseFoundation", new Vector3(-55.25951f, -1.23748684f, -24.0218639f), new Vector3(0, 0, 0));
              // mplayer1.buildItem("BaseFoundation", new Vector3(-52f, -4.6f, -21.6f), new Quaternion(0, 1, 0, 0));
               // Console.ReadLine();
              //  mplayer1.changeConstructionAmount(new Vector3(-52f, -4.6f, -21.6f), 0.5f);
              //  Console.ReadLine();
              //  mplayer1.changeConstructionAmount(new Vector3(-52f, -4.6f, -21.6f), 1f);
            }

        }
    }
}
