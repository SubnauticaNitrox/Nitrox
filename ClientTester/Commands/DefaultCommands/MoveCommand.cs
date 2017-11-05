using System;
using System.Timers;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveCommand : NitroxCommand
    {
        public MoveCommand()
        {
            Name = "move";
            Description = "Allows you to move the player with the mouse.";
            Syntax = "move";
        }

        private int lastX = -1;
        private int lastY = -1;
        public override void Execute(MultiplayerClient client, string[] args)
        {
            Console.WriteLine("Mouse is now attached. Press any key to exit");
            Timer mouseTimer = new Timer();
            mouseTimer.Elapsed += delegate
            {
                MouseTimerTick(client);
            };
            mouseTimer.Interval = 50;
            mouseTimer.Start();
            Console.ReadKey();
            lastX = -1;
            lastY = -1;
            mouseTimer.Stop();
        }

        private void MouseTimerTick(MultiplayerClient client)
        {
            int curX = System.Windows.Forms.Cursor.Position.X;
            int curY = System.Windows.Forms.Cursor.Position.Y;
            if (lastX != -1)
            {
                float velX = curX - lastX;
                float velY = curY - lastY;
                Vector3 velocity = new Vector3(velX / 10f, 0, velY / 10f);
                client.ClientPos += velocity;
                client.Logic.Player.UpdateLocation(client.ClientPos, velocity, Quaternion.identity, Quaternion.identity, Optional<VehicleModel>.Empty(), Optional<string>.Empty());
            }
            lastX = curX;
            lastY = curY;
        }
    }
}
