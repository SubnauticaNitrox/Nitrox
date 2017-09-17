using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeColorProcessor : ClientPacketProcessor<CyclopsChangeColor>
    {
        private PacketSender packetSender;

        public CyclopsChangeColorProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeColor colorPacket)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(colorPacket.Guid);

            if (opCyclops.IsPresent())
            {
                CyclopsNameScreenProxy screenProxy = opCyclops.Get().GetComponentInChildren<CyclopsNameScreenProxy>();

                if (screenProxy != null)
                {
                    SubName subname = (SubName)screenProxy.subNameInput.ReflectionGet("target");

                    if (subname != null)
                    {
                        Vector3 hsb = ApiHelper.Vector3(colorPacket.HSB);
                        Color color = ApiHelper.Color(colorPacket.Color);
                        subname.SetColor(colorPacket.Index, hsb, color);
                        screenProxy.subNameInput.ReflectionCall("SetColor", false, false, new object[] { colorPacket.Index, color });
                        screenProxy.subNameInput.SetSelected(colorPacket.Index);
                    }
                    else
                    {
                        Console.WriteLine("Could not find SubName in SubNameInput to change color");
                    }
                }
                else
                {
                    Console.WriteLine("Could not find CyclopsNameScreenProxy in cyclops to change color");
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + colorPacket.Guid + " to change color");
            }

        }
    }
}