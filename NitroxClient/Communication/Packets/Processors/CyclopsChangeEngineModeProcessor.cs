using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeEngineModeProcessor : ClientPacketProcessor<CyclopsChangeEngineMode>
    {
        private PacketSender packetSender;

        public CyclopsChangeEngineModeProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeEngineMode motorPacket)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(motorPacket.Guid);

            if (opCyclops.IsPresent())
            {
                CyclopsMotorMode motorMode = opCyclops.Get().GetComponentInChildren<CyclopsMotorMode>();
                CyclopsMotorModeButton[] motorModeButtons = opCyclops.Get().GetComponentsInChildren<CyclopsMotorModeButton>();

                if (motorMode != null & motorModeButtons != null)
                {
                    if ((CyclopsMotorMode.CyclopsMotorModes)motorPacket.Mode != motorMode.cyclopsMotorMode)
                    {
                        using (packetSender.Suppress<CyclopsChangeEngineMode>())
                        {
                            foreach (CyclopsMotorModeButton motorModeButton in motorModeButtons)
                            {
                                if (Player.main.currentSub == (SubRoot) motorModeButton.ReflectionGet("subRoot"))
                                {
                                    motorModeButton.SetCyclopsMotorMode((CyclopsMotorMode.CyclopsMotorModes)motorPacket.Mode);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Could not find CyclopsMotorMode or CyclopsMotorModeButton to change Motormode");
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + motorPacket.Guid + " to change Motormode");
            }
        }
    }
}
