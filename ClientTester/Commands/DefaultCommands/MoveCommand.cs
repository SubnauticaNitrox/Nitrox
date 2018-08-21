﻿using System;
using System.Timers;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveCommand : NitroxCommand
    {
        private readonly LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

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
                localPlayer.UpdateLocation(client.ClientPos, velocity, Quaternion.identity, Quaternion.identity, Optional<VehicleMovementData>.Empty());
            }

            lastX = curX;
            lastY = curY;
        }
    }
}
