using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication;
using UnityEngine;

namespace ClientTester
{
    class Program
    {
        static void Main(string[] args)
        {
            String playerId1 = "sunrunner";
            String playerId2 = "happyplace";

            System.Threading.Thread.Sleep(1000);
            MultiplayerClient mplayer1 = new MultiplayerClient(playerId1);
           // MultiplayerClient mplayer2 = new MultiplayerClient(playerId2);

            mplayer1.MockedPlayerPosition = new NitroxModel.DataStructures.Vector3(-48.96107f, -3.074013f, -42.54578f);
            //mplayer2.MockedPlayerPosition = new NitroxModel.DataStructures.Vector3(-48.96107f, -3.074013f, -42.54578f);

            while (true)
            {
                Console.ReadLine();
                 mplayer1.pickupItem(new Vector3(-50.6f, -4.8f, -38.0f), "Coral_reef_purple_mushrooms_01_04(Clone)", "AcidMushroom");
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
